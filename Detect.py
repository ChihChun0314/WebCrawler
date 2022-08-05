import datetime
from posixpath import split
import pymssql
from bs4 import BeautifulSoup
import sys
import requests
import re
import jieba
import jieba.posseg as pseg

url = sys.argv[1]
webName = sys.argv[2]
U_ID = int(sys.argv[3])
typeID = int(sys.argv[4])
#url = 'http://tw.class.uschoolnet.com/class/?csid=css000000015149&id=memstu&cl=1155125037-7732-6037'

res = requests.get(url)
html_page = res.content
soup = BeautifulSoup(html_page, 'html.parser')
text = soup.find_all(text=True)
    
db = pymssql.connect(
    host='127.0.0.1',
    user='sa',
    password = 'pat900518',
    database = 'Crawler'
)

output = ''
blacklist = [
    '[document]',
    'noscript',
    'header',
    'html',
    'meta',
    'head', 
    'input',
    'script',
    'style'

]
count = 0
for t in text:
    if t.parent.name not in blacklist:
        text = t.text.strip()
        t.string = re.sub(r"[\n][\W]+[^\w]", "\n", text)
        output += '{} '.format(text)
        count += 1

#詞性標註，nr為人名
def getFirstName(messageContent):
    words = pseg.cut(messageContent)
    for word, flag in words:
        if (flag == 'nr'and len(word) > 3):
            return word

    return False

def getAllName(messageContent):
    words = pseg.cut(messageContent)
    names = []
    for word, flag in words:
        # print('%s,%s' %(word,flag))
        if flag == 'nr':#人名詞性為nr
            names.append(word)
    return names

#修改停用詞集合中所有詞性為名詞，大部分為名詞
def alterWordTagToX(list):
    for x in list:
        jieba.add_word(x, tag='n')

def LoadStopWord():
    StopWordList = ["暱稱", "國小", "詳細資料", "博士班", "智慧型", "國立", "科技", "大學", "雲林", "寶貴"]

    set(StopWordList)
    alterWordTagToX(StopWordList)

phonePattern = r"[(]?([+]886[-\.\s]?[2-8]|0[2-8])[)]?[-\.\s]?\d{3,4}[-\.\s]?\d{3,4}|(\d{4}|[+]886[-\.\s]?\d{3})[-\.\s]??\d{3}[-\.\s]??\d{3}"

emailPattern = r"[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+"

if (typeID == 1):
    cnt = 0
    cursor = db.cursor()
    sql = "INSERT INTO Crawler (U_ID, Content, Time, URL, Web_name) VALUES (%s, %s, %s, %s, %s)"
    cursor.execute(sql, (U_ID, output, datetime.datetime.now(), url, webName))
    db.commit()

    cursor.execute("Select TOP(1) C_ID FROM Crawler WHERE U_ID ={} ORDER BY Time DESC".format(U_ID))
    row = cursor.fetchone()
    cid = int(row[0]) # Crawler ID

    LoadStopWord()
    final = []
    names = getAllName(output)

    for n in names:
        if(len(n) == 3 and not n in final):
            final.append(n)

    postOutput = ""
    for x in final:
        postOutput += "{}, ".format(x)
        cnt += 1

    sql = "INSERT INTO Analysis (C_ID, T_ID, Content, Count) VALUES (%s, %s, %s, %s)"
    cursor.execute(sql, (cid, typeID, postOutput, cnt))
    db.commit()
elif(typeID == 2):
    cnt = 0
    cursor = db.cursor()
    sql = "INSERT INTO Crawler (U_ID, Content, Time, URL, Web_name) VALUES (%s, %s, %s, %s, %s)"
    cursor.execute(sql, (U_ID, output, datetime.datetime.now(), url, webName))
    db.commit()

    cursor.execute("Select TOP(1) C_ID FROM Crawler WHERE U_ID ={} ORDER BY Time DESC".format(U_ID))
    row = cursor.fetchone()
    cid = int(row[0]) # Crawler ID

    Empty_list = []
    regex_ex = re.finditer(phonePattern, output, re.MULTILINE)
    for x in regex_ex:
        Empty_list.append(x.group(0))

    y = []

    for x in Empty_list:
        if(x not in y):
            y.append(x)

    postOutput = ""
    for x in y:
        postOutput += "{}, ".format(x)
        cnt += 1

    sql = "INSERT INTO Analysis (C_ID, T_ID, Content, Count) VALUES (%s, %s, %s, %s)"
    cursor.execute(sql, (cid, typeID, postOutput, cnt))
    db.commit()
elif(typeID == 3):
    cnt = 0
    cursor = db.cursor()
    sql = "INSERT INTO Crawler (U_ID, Content, Time, URL, Web_name) VALUES (%s, %s, %s, %s, %s)"
    cursor.execute(sql, (U_ID, output, datetime.datetime.now(), url, webName))
    db.commit()

    cursor.execute("Select TOP(1) C_ID FROM Crawler WHERE U_ID ={} ORDER BY Time DESC".format(U_ID))
    row = cursor.fetchone()
    cid = int(row[0]) # Crawler ID

    Empty_list = []
    regex_ex = re.finditer(emailPattern, output, re.MULTILINE)
    for x in regex_ex:
        Empty_list.append(x.group(0))

    y = []

    for x in Empty_list:
        if(x not in y):
            y.append(x)

    postOutput = ""
    for x in y:
        postOutput += "{}, ".format(x)
        cnt += 1

    sql = "INSERT INTO Analysis (C_ID, T_ID, Content, Count) VALUES (%s, %s, %s, %s)"
    cursor.execute(sql, (cid, typeID, postOutput, cnt))
    db.commit()


# print(output)
# print(count)