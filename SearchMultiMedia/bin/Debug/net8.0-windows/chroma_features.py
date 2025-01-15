import numpy as np
import os
import librosa

def custom_stft(y, frame_length=2048, hop_length=512):
    # Hàm tự viết STFT
    n_frames = 1 + (len(y) - frame_length) // hop_length
    frames = np.lib.stride_tricks.as_strided(
        y,
        shape=(frame_length, n_frames),
        strides=(y.strides[0], hop_length * y.strides[0])
    )
    window = np.hanning(frame_length)
    stft_matrix = np.fft.rfft(frames * window[:, None], axis=0)
    return np.abs(stft_matrix)

def chroma_features(y, sr, frame_length=2048, hop_length=512):
    D = custom_stft(y, frame_length=frame_length, hop_length=hop_length)
    freqs = np.fft.rfftfreq(frame_length, d=1/sr)

    # Loại bỏ tần số bằng 0 để tránh lỗi trong tính toán log2
    freqs[freqs == 0] = 1e-10  # Thay thế giá trị 0 bằng một giá trị nhỏ

    pitches = 12 * np.log2(freqs / 440.0)
    pitches = np.round(pitches) % 12

    chroma_matrix = np.zeros((12, D.shape[1]))
    for pitch_class in range(12):
        chroma_matrix[pitch_class] = np.sum(D[pitches == pitch_class], axis=0)

    max_vals = chroma_matrix.max(axis=0)
    max_vals[max_vals == 0] = 1  # Tránh chia cho 0
    chroma_matrix /= max_vals  # Chuẩn hóa theo cột

    chroma_list = chroma_matrix.mean(axis=0).tolist()
    return chroma_list

def calculate_new_features(fileAudio):
    try:
        if not os.path.isfile(fileAudio):
            raise FileNotFoundError(f"File {fileAudio} not found.")

        y, sr = librosa.load(fileAudio, sr=None)

        chromas = chroma_features(y, sr)

        with open('chroma_features.txt', 'w', encoding='utf-8') as myFile:
            myFile.write('\n'.join(map(str, chromas)))

    except Exception as e:
        print(f"Error in calculate_new_features: {str(e)}")

if __name__ == '__main__':
    import sys

    if len(sys.argv) != 2:
        print("Usage: python chroma_features.py <audio_file_path>")
    else:
        calculate_new_features(sys.argv[1])
