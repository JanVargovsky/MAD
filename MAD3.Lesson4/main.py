import pandas as pd
import sklearn.cluster as cl
import matplotlib.pyplot as plt
from scipy.cluster import hierarchy

orig_data = pd.read_csv('../Datasets/CC_GENERAL.csv', encoding='windows-1250')
df = orig_data.dropna()
df = df.drop(['CUST_ID', 'BALANCE_FREQUENCY', 'PRC_FULL_PAYMENT'], axis=1)
df = df.drop(['TENURE'], axis=1)


def generate_dendrogram():
    Z = hierarchy.linkage(df, method='ward')
    threshold = 210_000
    hierarchy.dendrogram(Z, no_labels=True, color_threshold=threshold)
    plt.savefig('Dendrogram-{0}.png'.format(threshold), dpi=500)
    plt.show()


columns = df.columns.values
N = 5
colors = {0: 'b', 1: 'r', 2: 'g', 3: 'y', 4: 'm'}
km = cl.KMeans(n_clusters=N).fit(df)
df['cluster'] = km.labels_
df['cluster_color'] = df['cluster'].map(colors)
grouped = df.groupby('cluster')


def generate_attributes():
    for ci, c1 in enumerate(columns):
        plt.suptitle(c1)
        plt.gcf().set_size_inches(10, 10)
        for i, c2 in enumerate(columns, 1):
            plt.subplot(4, 4, i)
            plt.xticks([])
            plt.yticks([])
            # plt.title(c2, size=8)
            plt.xlabel(c1, size=8)
            plt.ylabel(c2, size=8)
            plt.scatter(df[c1], df[c2], c=df['cluster_color'], marker='.', s=8)
        plt.savefig('Attributes/{0}_{1}.png'.format(ci, c1), dpi=300)
        plt.show()


def generate_boxplots():
    for ci, c in enumerate(columns):
        t = []
        for i, cluster in grouped:
            t.append(cluster[c])
        #plt.xticks(rotation=90)
        plt.title(c)
        plt.boxplot(t)
        plt.savefig('Boxplots/{0}_{1}.png'.format(ci, c), dpi=300)
        plt.show()


generate_dendrogram()
generate_attributes()
generate_boxplots()
