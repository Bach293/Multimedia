import requests
from bs4 import BeautifulSoup
import csv
import re 

BASE_URL = "https://vnexpress.net"

def clean_text(text):
    text = text.replace("'", " ").replace('"', " ").replace('-', " ").replace('.', " ").replace(',', " ").replace(';', " ")
    text = text.replace('(', " ").replace(')', " ").replace('[', " ").replace(']', " ").replace('{', " ").replace('}', " ")
    text = text.replace('?', " ").replace('!', " ").replace(':', " ")    
    text = text.replace("\n", " ").replace("\r", " ")
    text = re.sub(r'\s+', ' ', text)  
    text = text.strip()  # Loại bỏ khoảng trắng ở đầu và cuối
    return text

def get_html(url):
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
    }
    response = requests.get(url, headers=headers)
    response.raise_for_status()
    return response.text

def get_articles_from_category(category_url, max_pages=5):
    articles = []
    for page in range(1, max_pages + 1):
        print(f"Đang lấy dữ liệu từ {category_url} - Trang {page}...")
        page_url = f"{category_url}-p{page}"
        html = get_html(page_url)
        soup = BeautifulSoup(html, "html.parser")

        for article in soup.select(".item-news"):
            title_tag = article.select_one(".title-news a")
            description_tag = article.select_one(".description a")
            if title_tag and description_tag:
                title = clean_text(title_tag.get_text(" ", strip=True))
                description = clean_text(description_tag.get_text(" ", strip=True))
                link = title_tag["href"]
                articles.append({"title": title, "description": description, "link": link, "category": category_url.split("/")[-1]})
    return articles

def get_article_content(article_url):
    html = get_html(article_url)
    soup = BeautifulSoup(html, "html.parser")
    content = []
    for paragraph in soup.select(".fck_detail p"):
        text = clean_text(paragraph.get_text(separator=" ", strip=True)) + " "
        if text:
            content.append(text)
    return "\n".join(content)

categories = [
    f"{BASE_URL}/thoi-su",
    f"{BASE_URL}/the-gioi",
    f"{BASE_URL}/kinh-doanh",
    f"{BASE_URL}/giai-tri",
    f"{BASE_URL}/the-thao",
    f"{BASE_URL}/phap-luat",
    f"{BASE_URL}/giao-duc",
    f"{BASE_URL}/suc-khoe",
    f"{BASE_URL}/doi-song",
    f"{BASE_URL}/du-lich",
    f"{BASE_URL}/khoa-hoc",
    f"{BASE_URL}/so-hoa",
    f"{BASE_URL}/oto-xe-may",
    f"{BASE_URL}/y-kien",
    f"{BASE_URL}/tam-su"
]

all_articles = []
for category_url in categories:
    articles = get_articles_from_category(category_url, max_pages=1)  
    for article in articles:
        print(f"Đang lấy nội dung bài viết: {article['title']}")
        try:
            article["content"] = get_article_content(article["link"])
        except Exception as e:
            print(f"Lỗi khi lấy nội dung bài viết: {e}")
            article["content"] = ""
    all_articles.extend(articles)

csv_file = "vnexpress_all_categories.csv"
with open(csv_file, mode="w", encoding="utf-8", newline="") as file:
    writer = csv.DictWriter(file, fieldnames=["title", "description", "link", "category", "content"])
    writer.writeheader()
    writer.writerows(all_articles)

print(f"Hoàn tất! Dữ liệu đã được lưu tại {csv_file}.")
