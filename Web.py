from posixpath import split
import pymssql
from bs4 import BeautifulSoup
import sys
import requests
first = sys.argv[1]
# first = input()
# url = "https://www.yuntech.edu.tw/index.php"
r = requests.get(first)
soup = BeautifulSoup(r.text, 'html.parser')
cardcontent = soup.find_all(
    'div')

card = [e.get_text() for e in cardcontent]

db = pymssql.connect(
    host='127.0.0.1',
    user='sa',
    password='1234',
    database='yuntech'
)
e = ""
ans = ""
for i in range(0, len(card)):
    e = card[i].replace("\n", "")
    e = e.replace("\t", "")
    for x in e:
        ans += x

cursor = db.cursor()
cursor.execute("INSERT INTO information (cardcontent) VALUES (%s)", (ans))
db.commit()
