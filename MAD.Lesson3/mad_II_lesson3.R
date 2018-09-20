############################
# MAD I CV 2
#df<-read.csv("./data/KarateClub.csv", header = F, sep = ";", dec = ",")
#install.packages("igraph")
library(igraph)
g1 <- make_graph("Zachary")
g1

if (!require("igraph")) 
  install.packages("igraph")
library(igraph)

#g1 <- graph_from_data_frame(d = df, directed = F)
class(g1) # urceni "typu" datoveho objektu 
E(g1)
V(g1)
plot(g1)

as_edgelist(g1, names = F) # muzeme mit take seznam hran
A <- as_adjacency_matrix(g1, names = FALSE) # nebo matici sousednosti
print(A)

plot(g1, layout = layout_randomly)
plot(g1, layout = layout_in_circle)
plot(g1, layout = layout_on_sphere)
plot(g1, layout = layout_with_fr)

deg <- degree(g1, mode="all")
plot(g1, vertex.size=deg * 2)
print(deg)
deg["34"] 
min(deg)
max(deg)
mean(deg) # prumerny stupen
hist(deg, breaks = 1:vcount(g1)-1, main = "Histogram stupnu", xlab = "stupen", ylab = "cetnost")

barplot(table(deg))

# pomoci ggplot2
library(ggplot2)
t.deg <- table(deg) # dostaneme cetnosti
df2 <- data.frame(degree = as.numeric(names(t.deg)),freq = as.numeric(t.deg)) # ggplot chce data.frame
ggplot(df2,aes(x = degree, y = freq)) + geom_bar(stat = "identity", width = 0.4) + scale_x_continuous(breaks=scales::pretty_breaks(n = 17)) + scale_y_continuous(breaks=scales::pretty_breaks(n = 12)) 

# relativni cetnosti
deg_rel <- degree_distribution(g1, mode = "all")
# vykreslime
ggplot()+geom_point(aes(c(0:(length(deg_rel)-1)),deg_rel))+ scale_x_continuous(breaks=c(0:17)) + scale_y_continuous(breaks=scales::pretty_breaks(n = 12)) + labs(x = "degree", y = "freq.") + theme_bw()
# pokud nechceme stupen == 0
ggplot()+geom_point(aes(c(1:(length(deg_rel)-1)),deg_rel[-1])) + scale_x_continuous(breaks=c(1:17)) + scale_y_continuous(breaks=scales::pretty_breaks(n = 12)) + labs(x = "degree", y = "freq.") + theme_bw()

# kumulativni distribuce 
deg.dist <- degree_distribution(g1, cumulative=T, mode="all")
plot(x=0:max(deg), y=1-deg.dist, pch=19, cex=1.2, col="orange", xlab="Degree", ylab="Cumulative Frequency")

# kumulativni distribuce, ale pomoci CCDF, Complementary cumulative distribution function, P(X > x)!!!!! 
deg.dist.CCDF <- 1 - degree_distribution(g1, cumulative=T, mode="all")
plot(x=0:max(deg), y=1-deg.dist.CCDF, pch=19, cex=1.2, col="orange", xlab="Degree", ylab="Cumulative Frequency")

############################
# MAD II CV 4.4.
library(igraph)
library(lsa)

g = make_graph("Zachary") # nacti Karate club
coords = layout_with_fr(g) # jaka je to lazoutivaci metoda?
plot(g, layout=coords, vertex.label=NA, vertex.size=10)

############################
# greedy method (hiearchical, fast method)
c1 = cluster_fast_greedy(g)

membership(c1)
length(c1)
sizes(c1)
crossing(c1, g)
plot(c1, g, layout=coords) 
plot(g, vertex.color=membership(c1), layout=coords) # vyexportujte
plot_dendrogram(c1) # vyexportujte

############################
# Sami pro dalsi metody
c2 = cluster_leading_eigen(g) # jaka je to metoda ?
plot(c2, g, layout=coords)
plot_dendrogram(c2)

############################
# Sami 
c3 = cluster_edge_betweenness(g) # jaka je to metoda ?
plot(c3, g, layout=coords) 
plot_dendrogram(c3)

############################
# Sami 
c4 = cluster_optimal(g) # jaka je to metoda ?
plot(c4, g, layout=coords) 

############################
# hierarchicke shlukovani

S=similarity(g, method = "invlogweighted") # co to je za metodu ("invlogweighted")?
D = 1-S

# vyzkousejte i dalsi dve metody pro vypocet podobnosti

# distance object
d = as.dist(D)

# average-linkage clustering method
cc = hclust(d, method = "average")

# plot dendrogram
plot(cc)

# draw blue borders around clusters
clusters.list = rect.hclust(cc, k = 4, border="blue") # riznete i pro k = 3, k = 2

# cut dendrogram at 4 clusters
clusters = cutree(cc, k = 4)

plot(g, vertex.color=clusters, layout=coords)

#######################################

# global similarity
I = diag(1, vcount(g));

# leading eigenvalue
l = eigen(A)$values[1]

# 85% of leading eigenvalue
alpha = 0.85 * 1/l

# similarity matrix
S = solve(I - alpha * A)

# largest value
max = max(as.vector(S))

# distance matrix
D = max-S

# set null distance from a node to itself
d = diag(D)
D = D + diag(-d, vcount(g))


# distance object
d = as.dist(D)

# average-linkage clustering method
cc = hclust(d, method = "average")

# plot dendrogram
plot(cc)

# draw blue borders around clusters
clusters.list = rect.hclust(cc, k = 4, border="blue")

# cut dendrogram 
clusters = cutree(cc, k = 4)

# plot graph with clusters
plot(g, vertex.color=clusters, layout=coords)

# cviko 11.4.2018
coreness(g)

c5 = cluster_louvain(g)
plot(c5, g, layout=coords)

C = c(c1,c2,c3,c4,c5)

m1 = membership(c1)
modularity(g, m1)

m2 = membership(c2)
modularity(g, m2)

m3 = membership(c3)
modularity(g, m3)

m4 = membership(c4)
modularity(g, m4)

m5 = membership(c5)
modularity(g, m5)

cli = cliques(g, 3, 5)
for(i in 1:2)
{
  plot(induced.subgraph(graph=g,vids=cli[[i]]))
}