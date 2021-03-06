import pandas as pd
import matplotlib.pyplot as plt

FILENAMES = ("clusters3.csv", "clusters5.csv", "clusters5n.csv", "annulus.csv", "boxes.csv")
ALGORITHMS = ("SingleLinkage", "CompleteLinkage")
COLUMN_NAMES = ('x', 'y', 'class')
COLORS = ("b", "g", "r", "y", "m")

for FILENAME in FILENAMES:
    for (i, ALGORITHM) in enumerate(ALGORITHMS):
        filename = "Data/{0}-{1}".format(ALGORITHM, FILENAME)
        frame = pd.read_csv(filename, delimiter=';', encoding='windows-1250', names=COLUMN_NAMES)
        grouped = frame.groupby('class')
        plt.subplot(2, 1, i+1)
        plt.title('{0} - {1}'.format(FILENAME, ALGORITHM))
        plt.gcf().set_size_inches(5, 10)
        for ((_, group), color) in zip(grouped, COLORS):
            plt.scatter(group['x'], group['y'], c=color, marker=".")
    plt.savefig('Data/Export-{0}.png'.format(FILENAME), dpi=300)
    plt.show()
