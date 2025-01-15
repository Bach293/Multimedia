import os
import numpy as np
import cv2

class HuMomentFeatures:
    def _moments(self, image):
        m = {}
        rows, cols = image.shape
        for p in range(4):
            for q in range(4):
                m[(p, q)] = np.sum((np.arange(rows)[:, None] ** p) * (np.arange(cols)[None, :] ** q) * image)
        return m

    def _central_moments(self, image, moments):
        rows, cols = image.shape
        x_bar = moments[(1, 0)] / moments[(0, 0)]
        y_bar = moments[(0, 1)] / moments[(0, 0)]

        mu = {}
        for p in range(4):
            for q in range(4):
                mu[(p, q)] = np.sum(
                    ((np.arange(rows)[:, None] - x_bar) ** p)
                    * ((np.arange(cols)[None, :] - y_bar) ** q)
                    * image
                )
        return mu

    def _hu_moments(self, central_moments):
        hu = [0] * 7
        cm = central_moments

        hu[0] = cm[(2, 0)] + cm[(0, 2)]
        hu[1] = (cm[(2, 0)] - cm[(0, 2)]) ** 2 + 4 * (cm[(1, 1)] ** 2)
        hu[2] = (cm[(3, 0)] - 3 * cm[(1, 2)]) ** 2 + (3 * cm[(2, 1)] - cm[(0, 3)]) ** 2
        hu[3] = (cm[(3, 0)] + cm[(1, 2)]) ** 2 + (cm[(2, 1)] + cm[(0, 3)]) ** 2
        hu[4] = (cm[(3, 0)] - 3 * cm[(1, 2)]) * (cm[(3, 0)] + cm[(1, 2)]) * (
            (cm[(3, 0)] + cm[(1, 2)]) ** 2
            - 3 * (cm[(2, 1)] + cm[(0, 3)]) ** 2
        ) + (3 * cm[(2, 1)] - cm[(0, 3)]) * (cm[(2, 1)] + cm[(0, 3)]) * (
            3 * (cm[(3, 0)] + cm[(1, 2)]) ** 2 - (cm[(2, 1)] + cm[(0, 3)]) ** 2
        )
        hu[5] = (cm[(2, 0)] - cm[(0, 2)]) * (
            (cm[(3, 0)] + cm[(1, 2)]) ** 2 - (cm[(2, 1)] + cm[(0, 3)]) ** 2
        ) + 4 * cm[(1, 1)] * (cm[(3, 0)] + cm[(1, 2)]) * (cm[(2, 1)] + cm[(0, 3)])
        hu[6] = (
            (3 * cm[(2, 1)] - cm[(0, 3)]) * (cm[(3, 0)] + cm[(1, 2)]) * (
                (cm[(3, 0)] + cm[(1, 2)]) ** 2 - 3 * (cm[(2, 1)] + cm[(0, 3)]) ** 2
            )
            - (cm[(3, 0)] - 3 * cm[(1, 2)]) * (cm[(2, 1)] + cm[(0, 3)]) * (
                3 * (cm[(3, 0)] + cm[(1, 2)]) ** 2 - (cm[(2, 1)] + cm[(0, 3)]) ** 2
            )
        )
        return np.array(hu)

    def extract(self, image):
        if len(image.shape) == 3:
            image = image.mean(axis=2)  

        moments = self._moments(image)
        central_moments = self._central_moments(image, moments)
        hu_moments = self._hu_moments(central_moments)

        return -np.sign(hu_moments) * np.log(np.abs(hu_moments) + 1e-7)

class GaborFeatures:
    def __init__(self, ksize=31, sigma=4.0, lambd=10.0, gamma=0.5, psi=0):
        self.ksize = ksize
        self.sigma = sigma
        self.lambd = lambd
        self.gamma = gamma
        self.psi = psi
        self.thetas = [0, np.pi / 4, np.pi / 2, 3 * np.pi / 4]

    def _gabor_kernel(self, theta):
        sigma_x = self.sigma
        sigma_y = self.sigma / self.gamma

        xmax = ymax = self.ksize // 2
        xmin = ymin = -xmax

        x, y = np.meshgrid(np.arange(xmin, xmax + 1), np.arange(ymin, ymax + 1))

        x_theta = x * np.cos(theta) + y * np.sin(theta)
        y_theta = -x * np.sin(theta) + y * np.cos(theta)

        exp_part = np.exp(-0.5 * ((x_theta**2) / sigma_x**2 + (y_theta**2) / sigma_y**2))
        cos_part = np.cos(2 * np.pi * x_theta / self.lambd + self.psi)

        return exp_part * cos_part

    def extract(self, image):
        if len(image.shape) == 3:
            image = image.mean(axis=2) 

        features = []
        for theta in self.thetas:
            kernel = self._gabor_kernel(theta)
            filtered = self._filter_image(image, kernel)
            features.append(filtered.mean())
            features.append(filtered.var())
        return np.array(features)

    def _filter_image(self, image, kernel):
        return cv2.filter2D(image, -1, kernel)

def calculate_features(image_path):
    try:
        if not os.path.isfile(image_path):
            raise FileNotFoundError(f"File {image_path} not found.")

        image = cv2.imread(image_path, cv2.IMREAD_GRAYSCALE)
        if image is None:
            raise ValueError("Failed to read the image file.")

        hu_extractor = HuMomentFeatures()
        gabor_extractor = GaborFeatures()

        hu_features = hu_extractor.extract(image)
        gabor_features = gabor_extractor.extract(image)

        with open('hu_moment_features.txt', 'w', encoding='utf-8') as hu_file:
            hu_file.write('\n'.join(map(str, hu_features)))

        with open('gabor_features.txt', 'w', encoding='utf-8') as gabor_file:
            gabor_file.write('\n'.join(map(str, gabor_features)))

    except Exception as e:
        print(f"Error in calculate_features: {str(e)}")

if __name__ == '__main__':
    import sys

    if len(sys.argv) != 2:
        print("Usage: python calculate_image_features.py <image_file_path>")
    else:
        calculate_features(sys.argv[1])
