import librosa
import numpy as np
import os

def spectral_centroid(y, sr, frame_length=2048, hop_length=512):
    D = librosa.stft(y, n_fft=frame_length, hop_length=hop_length)
    magnitude, _ = librosa.magphase(D)  # Lấy phần magnitude của phổ

    centroid_list = []
    for t in range(magnitude.shape[1]):  # Duyệt qua từng khung
        # Tính trọng tâm phổ cho mỗi khung
        numerator = np.sum(np.arange(magnitude.shape[0]) * magnitude[:, t])
        denominator = np.sum(magnitude[:, t])
        centroid = numerator / denominator if denominator != 0 else 0
        centroid_list.append(centroid)
    
    return centroid_list

def spectral_bandwidth(y, sr, frame_length=2048, hop_length=512):
    D = librosa.stft(y, n_fft=frame_length, hop_length=hop_length)
    magnitude, _ = librosa.magphase(D)  # Lấy phần magnitude của phổ

    # Tính Spectral Centroid
    centroids = spectral_centroid(y, sr, frame_length, hop_length)
    
    bandwidth_list = []
    for t in range(magnitude.shape[1]):  # Duyệt qua từng khung
        centroid = centroids[t]
        # Tính Spectral Bandwidth bằng công thức
        numerator = np.sum(((np.arange(magnitude.shape[0]) - centroid) ** 2) * magnitude[:, t])
        denominator = np.sum(magnitude[:, t])
        bandwidth = np.sqrt(numerator / denominator) if denominator != 0 else 0
        bandwidth_list.append(bandwidth)
    
    return bandwidth_list

def calculate_features(fileAudio):
    try:
        if not os.path.isfile(fileAudio):
            raise FileNotFoundError(f"File {fileAudio} not found.")
        
        y, sr = librosa.load(fileAudio, sr=None)
        
        centroids = spectral_centroid(y, sr)
        bandwidths = spectral_bandwidth(y, sr)
        
        with open('spectral_features_centroid.txt', 'w', encoding='utf-8') as myFile:
            myFile.write('\n'.join(map(str, centroids)))
        with open('spectral_features_bandwidths.txt', 'w', encoding='utf-8') as myFile:
            myFile.write('\n'.join(map(str, bandwidths)))
            
    except Exception as e:
        print(f"Error in calculate_features: {str(e)}")

if __name__ == '__main__':
    import sys

    if len(sys.argv) != 2:
        print("Usage: python cal_spectral_features.py <audio_file_path>")
    else:
        calculate_features(sys.argv[1])
