from DecisionTree import DecisionTree
from random import seed, choices
from collections import Counter

class RandomForest:
    def __init__(self, n_trees, seed=42):
        self.trees = []
        self.n_trees = n_trees
        self.seed = seed
        
    """
    Bootstrap aggregating - bagging
    """
    def _bagging(self, df):
        return df.loc[choices(df.index.values, k=len(df))]
        
    def fit(self, df):
        seed(self.seed)
        trees = []
        for i in range(self.n_trees):
            tree = DecisionTree()
            tree.fit(self._bagging(df))
            trees.append(tree)
        self.trees = trees
    
    def predict(self, values):
        predicted = [tree.predict(values) for tree in self.trees]
        return Counter(predicted).most_common()[0][0]
    
    def score(self, df):
        correct = 0
        for _, row in df.iterrows():
            predicted = self.predict(row)
            if predicted == row[-1]:
                correct += 1
        return correct / float(len(df))