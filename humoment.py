import numpy as np

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

        # Tính các Hu Moments
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
            image = image.mean(axis=2)  # Chuyển ảnh sang grayscale

        moments = self._moments(image)
        central_moments = self._central_moments(image, moments)
        hu_moments = self._hu_moments(central_moments)

        # Chuyển đổi log để dễ so sánh
        return -np.sign(hu_moments) * np.log(np.abs(hu_moments) + 1e-7)
