import cv2
import numpy as np
import csv
from scipy.spatial.distance import cosine
from gabor import GaborFeatures
from humoment import HuMomentFeatures
import tkinter as tk
from tkinter import filedialog
from tkinter import messagebox
from PIL import Image, ImageTk

# Hàm tính Histogram màu
def color_histogram(image, bins=32):
    hist_b = cv2.calcHist([image], [0], None, [bins], [0, 256])
    hist_g = cv2.calcHist([image], [1], None, [bins], [0, 256])
    hist_r = cv2.calcHist([image], [2], None, [bins], [0, 256])
    hist = np.concatenate([hist_b, hist_g, hist_r]).flatten()
    return hist / np.sum(hist)  # Chuẩn hóa

# Kết hợp đặc trưng
def combined_features(image, gabor_extractor, hu_extractor):
    color_features = color_histogram(image)
    texture_features = gabor_extractor.extract(image)
    shape_features = hu_extractor.extract(image)
    return np.concatenate([color_features, texture_features, shape_features])

# Hàm tính độ tương đồng cosine
def cosine_similarity(v1, v2):
    return 1 - cosine(v1, v2)

# Hàm tìm 3 ảnh tương đồng nhất
def find_top_3_similar_images(input_image, image_names, features, gabor_extractor, hu_extractor):
    # Trích xuất đặc trưng từ ảnh đầu vào
    input_features = combined_features(input_image, gabor_extractor, hu_extractor)

    similarities = []
    for idx, feature in enumerate(features):
        sim = cosine_similarity(input_features, feature)
        similarities.append((image_names[idx], sim))

    # Sắp xếp giảm dần theo độ tương đồng
    similarities.sort(key=lambda x: x[1], reverse=True)

    # Lấy 3 ảnh có độ tương đồng cao nhất
    return similarities[:3]

# Hàm hiển thị ảnh lên giao diện Tkinter
def show_image(image_path, label):
    img = Image.open(image_path)
    img = img.resize((250, 250))  # Kích thước ảnh hiển thị
    img = ImageTk.PhotoImage(img)
    label.config(image=img)
    label.image = img

# Hàm chọn ảnh và tính toán
def on_select_image():
    input_image_path = filedialog.askopenfilename(title="Chọn ảnh đầu vào", filetypes=[("Image files", "*.jpg;*.jpeg;*.png")])
    if not input_image_path:
        return

    input_image = cv2.imread(input_image_path)
    if input_image is None:
        messagebox.showerror("Error", "Không thể đọc ảnh đầu vào.")
        return

    # Đọc dữ liệu từ CSV
    image_names = []
    features = []
    feature_csv = "output_features2.csv"
    with open(feature_csv, mode='r') as file:
        reader = csv.reader(file)
        header = next(reader)

        for row in reader:
            image_names.append(row[0])
            features.append(np.array(row[1:], dtype=float))

    # Khởi tạo các extractor
    gabor_extractor = GaborFeatures()
    hu_extractor = HuMomentFeatures()

    # Tìm 3 ảnh tương đồng nhất
    top_3_images = find_top_3_similar_images(input_image, image_names, features, gabor_extractor, hu_extractor)

    # Hiển thị ảnh đầu vào
    show_image(input_image_path, input_image_label)

    # Hiển thị 3 ảnh tương đồng nhất
    for i, (image_name, similarity) in enumerate(top_3_images):
        image_path = f"images/{image_name}"  # Đảm bảo ảnh nằm trong thư mục 'images'
        if i == 0:
            show_image(image_path, result_label1)
        elif i == 1:
            show_image(image_path, result_label2)
        else:
            show_image(image_path, result_label3)

# Khởi tạo giao diện Tkinter
root = tk.Tk()
root.title("Image Similarity Finder")

# Cửa sổ giao diện
frame = tk.Frame(root)
frame.pack(padx=10, pady=10)

# Nút chọn ảnh và tính toán
select_button = tk.Button(frame, text="Chọn ảnh đầu vào", command=on_select_image)
select_button.pack()

# Nhãn để hiển thị ảnh đầu vào
input_image_label = tk.Label(frame)
input_image_label.pack(pady=10)

# Nhãn để hiển thị kết quả (3 ảnh tương đồng nhất)
result_label1 = tk.Label(frame)
result_label1.pack(side=tk.LEFT, padx=10)

result_label2 = tk.Label(frame)
result_label2.pack(side=tk.LEFT, padx=10)

result_label3 = tk.Label(frame)
result_label3.pack(side=tk.LEFT, padx=10)

# Chạy giao diện
root.mainloop()
