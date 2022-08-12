import datetime
from posixpath import split
import pymssql
from bs4 import BeautifulSoup
import sys
import requests
import re
import jieba
import jieba.posseg as pseg
import string
from string import whitespace
nonalpha = string.digits + string.punctuation + string.whitespace

url = sys.argv[1]
webName = sys.argv[2]
U_ID = int(sys.argv[3])
#typeID = int(sys.argv[4])
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
    StopWordList = ["暱稱", "國小", "詳細資料", "博士班", "智慧型", "國立", "科技", "大學", "雲林", "寶貴", "關於"]

    set(StopWordList)
    alterWordTagToX(StopWordList)

phonePattern = r"[(]?([+]886[-\.\s]?[2-8]|0[2-8])[)]?[-\.\s]?\d{3,4}[-\.\s]?\d{3,4}|(\d{4}|[+]886[-\.\s]?\d{3})[-\.\s]??\d{3}[-\.\s]??\d{3}"

emailPattern = r"[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+"

def func(value):
    return ''.join(value.splitlines())

city = [
    u'台北市', u'新北市', u'基隆市', u'宜蘭縣',
    u'新竹市', u'新竹縣', u'桃園市', u'苗栗縣',
    u'台中市', u'彰化縣', u'南投縣', u'雲林縣',
    u'嘉義市', u'嘉義縣', u'台南市', u'高雄市',
    u'屏東縣', u'台東縣', u'花蓮縣', u'澎湖縣',
    u'金門縣', u'連江縣'
]
other_city = [
    u'台北縣', u'高雄縣', u'臺北市', u'臺北縣',
    u'台中縣', u'臺中市', u'臺中縣'
]
chinese_digit = [
    u'一', u'二', u'三', u'四', u'五',
    u'六', u'七', u'八', u'九', u'十'
]


def extract_address(txt):
    addr_main = u'[%s?]\w+?[路|街|鄰]\w*?\s?\d{1,6}\s?號|\w+\s?號' % '|'.join(city + other_city)
    addr_optional = u'(?:\d{1,5}樓|\d+(?:、\d+){0,2}樓|\w+樓)?(?:\d{1,3}室\w+室)?(?:[\-|之]\d{1,2}\w)?'
    pattern = addr_main + addr_optional
    return re.findall(pattern, txt, re.UNICODE)

cursor = db.cursor()
sql = "INSERT INTO Crawler (U_ID, Content, Time, URL, Web_name) VALUES (%s, %s, %s, %s, %s)"
cursor.execute(sql, (U_ID, output, datetime.datetime.now(), url, webName))
db.commit()

cursor.execute("Select TOP(1) C_ID FROM Crawler WHERE U_ID ={} ORDER BY Time DESC".format(U_ID))
row = cursor.fetchone()
cid = int(row[0]) # Crawler ID

for typeID in range(1, 5):
    if (typeID == 1):
        cnt = 0
    
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
    elif(typeID == 4):
        cnt = 0

        out = func(output)
        out.translate(dict.fromkeys(map(ord, whitespace)))
        out.replace("\\n","")
        out.replace("\\\n","")
        out.replace("\\\\n","")

        res = extract_address(out)
        reg = re.compile(r'[a-zA-Z]')

        y = []

        for x in res:
            if(x not in y and len(x) > 8 and not reg.match(x) and ((any(char.isdigit() for char in x)) or any(i in x for i in chinese_digit)) and not re.search('[a-zA-Z]', x) and re.search('\w{2}[縣|市|區|鄉]', x)):
                x = re.sub('^[0-9]+', "", x)
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