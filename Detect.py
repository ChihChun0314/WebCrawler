import datetime
from posixpath import split
import pymssql
from bs4 import BeautifulSoup
import sys
import requests
import re

url = sys.argv[1]
webName = sys.argv[2]
U_ID = int(sys.argv[3])
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
    # there may be more elements you don't want, such as "style", etc.
]
count = 0
for t in text:
    if t.parent.name not in blacklist:
        text = t.text.strip()
        t.string = re.sub(r"[\n][\W]+[^\w]", "\n", text)
        output += '{} '.format(text)
        count += 1

cursor = db.cursor()
sql = "INSERT INTO Crawler (U_ID, Content, Time, URL, Web_name) VALUES (%s, %s, %s, %s, %s)"
cursor.execute(sql, (U_ID, output, datetime.datetime.now(), url, webName))
db.commit()

# print(output)
# print(count)