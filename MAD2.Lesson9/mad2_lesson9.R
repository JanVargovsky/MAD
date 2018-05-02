library(igraph)
# igraph
N = 5500
M = 3

### 1
# Barabasi-Albert Model
g1 = barabasi.game(n=N, m=M)
degree.distribution(g1)

# random
#erdos.renyi.game
g2 = sample_gnm(n=N, m=N*M*1.5)
degree.distribution(g2)

### 2
# a) pocet komponent
no.clusters(g1)
no.clusters(g2)

# b) velikosti komponent
clusters(g1)$csize
clusters(g2)$csize
# nejvetsi komponenta
max(clusters(g1)$csize)
max(clusters(g2)$csize)

# c) prumerna vzdalenost
mean_distance(g1, directed = FALSE)
mean_distance(g2, directed = FALSE)
# d) prumerny stupen
mean(degree(g1))
mean(degree(g2))


# 3
print_info <- function(g){
  print(paste("No nodes", vcount(g)))
  print(paste("No clusters", no.clusters(g)))
  print(paste("Clusters ", clusters(g)$csize))
  print(paste("Max Cluster ", max(clusters(g)$csize)))
  print(paste("Mean distance ", mean_distance(g, directed = FALSE)))
  print(paste("Mean degree ", mean(degree(g))))
}

# a
remove_node_with_highest_degree <- function(g){
  max = which.max(degree(g))
  g = delete.vertices(g, max)
  return (g)
}

# b
remove_random_node <- function(g){
  node_count = vcount(g)
  node = sample(0:node_count, 1)
  g = delete.vertices(g, node)
  return (g)
}

# c
call_3 <- function(g){
  remove_nodes = 5
  g_m = g
  print_info(g_m)
  for(i in 0:remove_nodes) {
    g_m = remove_node_with_highest_degree(g_m)
    print('========================================================================================================================')
    print_info(g_m)
  }
  
  g_m = g
  print_info(g_m)
  for(i in 0:remove_nodes) {
    g_m = remove_random_node(g_m)
    print('========================================================================================================================')
    print_info(g_m)
  }
}

call_5 <- function(g, title) {
  remove_nodes = vcount(g) - 1
  step = 100
  
  g_m = g
  values = c()
  values = append(values, max(clusters(g)$csize))
  nodes = c()
  nodes = append(nodes, 1)
  for(i in 1:remove_nodes/step){
    for (j in step){
      g_m = remove_node_with_highest_degree(g_m)
    }
    values = append(values, max(clusters(g_m)$csize))
    nodes = append(nodes, i*step)
  }
  plot(nodes, values, xlab = "pocet odstranenych vrcholu (s nejvetsim stupnem)", ylab="velikost maximalniho clusteru", main = title)
  
  g_m = g
  values = c()
  values = append(values, max(clusters(g_m)$csize))
  nodes = c()
  nodes = append(nodes, 1)
  for(i in 1:remove_nodes/step){
    for (j in step){
      g_m = remove_random_node(g_m)
    }
    values = append(values, max(clusters(g_m)$csize))
    nodes = append(nodes, i*step)
  }
  plot(nodes, values, xlab = "pocet odstranenych vrcholu (nahodny)", ylab="velikost maximalniho clusteru", main = title)
}

call_3(g1)
call_3(g2)

call_5(g1, "Barabasi-Albert Model")
call_5(g2, "Nahodny graf")

