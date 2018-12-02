class Node:
    def __init__(self, column, gini, value, samples):
        self.column = column
        self.gini = gini
        self.value = value
        self.samples = samples
        self.left = None
        self.right = None

    def __repr__(self):
        return "column={}, gini={}, value={}, samples={}".format(self.column, self.gini, self.value, self.samples)

    def is_leaf(self):
        return self.left is None and self.right is None

class DecisionTree:
    def __init__(self):
        self.root = None
    
    def _gini(self, classes):
        groups = {}
        for v in classes:
            if not v in groups:
                groups[v] = 0
            groups[v] = groups[v] + 1 
        #print("groups", groups)
        return 1 - sum(map(lambda p: (p / len(classes)) ** 2, groups.values()))

    def _weighted_gini(self, classes, split_index):
        g1 = self._gini(classes[:split_index])
        g2 = self._gini(classes[split_index:])
        #print("g1", g1, "g2", g2)
        g1w = g1 * (split_index / len(classes))
        g2w = g2 * ((len(classes) - split_index) / len(classes))
        #print("g1w", g1w, "g2w", g2w)
        return g1w + g2w

    def _find_best_gini(self, df):
        i = -1 # column
        g = 100 # worst gini
        s = -1 # split
        v = None # split value

        for column_index, column in enumerate(df.columns[:-1]):
            #print("COLUMN", column)
            df = df.sort_values(by=column)
            #display(df.head())
            for split in range(1, len(df)):
                #print(split)
                v1 = df.iloc[split - 1, column_index]
                #print("v1", v1)
                v2 = df.iloc[split, column_index]
                #print("v2", v2)
                if v1 == v2:
                    # edge case when there are only records with same value 
                    #print("Equal indexes {}".format(v1))
                    continue
                newG = self._weighted_gini(df[df.columns[-1]], split)
                if newG < g:
                    i, g, s, v = column, newG, split, (v1 + v2) / 2
                    #print("column=", i, "g=", g, "s=", s, "v=", v, v1, v2)
            #print("column=", i, "g=", g, "s=", s, "v=", v)
        #print("column=", i, "g=", g, "s=", s, "v=", v)
        return i, g, v
    
    def fit(self, df):
        def build_node(df):
            column, gini, value = self._find_best_gini(df)
            #display(df.head())
            node = Node(column, gini, value, len(df))
            #display(node)
            return node
        def build_node_recursively(parent, df, depth):
            #print("depth", depth, parent, df.columns)
            if len(df) == 0:
                #print("fail")
                return
            values = df[df.columns[-1]].unique()
            #print(values)
            if len(values) == 1:
                #print("leaf", values[0])
                parent.value = values[0]
                return

            left_df = df[df[parent.column] <= parent.value]
            parent.left = build_node(left_df)
            #print("build left")
            build_node_recursively(parent.left, left_df, depth + 1)

            right_df = df[df[parent.column] > parent.value]
            parent.right = build_node(right_df)
            #print("build right")
            build_node_recursively(parent.right, right_df, depth + 1)

        self.root = build_node(df)
        build_node_recursively(self.root, df, 0)
    
    def predict(self, values):
        node = self.root
        while not node.is_leaf():
            if values[node.column] <= node.value:
                #print("go left")
                node = node.left
            else:
                #print("go right")
                node = node.right
        return node.value
    
    def score(self, df):
        correct = 0
        for _, row in df.iterrows():
            predicted = self.predict(row)
            if predicted == row[-1]:
                correct += 1
        return correct / float(len(df))