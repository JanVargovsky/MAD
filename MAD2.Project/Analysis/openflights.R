# https://toreopsahl.com/datasets/#usairports
# https://www.rstudio.com/wp-content/uploads/2015/03/ggplot2-cheatsheet.pdf

# library(readr)
# openflights <-
#   read_delim(
#     "E:/Programming/School/MAD/MAD2.Project/openflights.txt",
#     " ",
#     escape_double = FALSE,
#     col_names = FALSE,
#     comment = "#",
#     trim_ws = TRUE,
#     col_types = list(col_integer(), col_integer(), col_integer())
#   )
# head(openflights)
# colnames(openflights) = c("From", "To", "Weight")
# openflights_matrix = as.matrix(openflights)
# library(igraph)
#g = graph.edgelist(openflights_matrix, directed = TRUE)

rm(list=ls())

library(igraph)
g = read.graph("E:/Programming/School/MAD/MAD2.Project/openflights.txt",
               format = "ncol")
nodes = read.table("E:/Programming/School/MAD/MAD2.Project/openflights_airports.txt",
                   header = T)

library(ggplot2)

paste("Pocet vrcholu:", vcount(g))
paste("Pocet hran:", ecount(g))
paste("Prumerna vzdalenost:", mean_distance(g, directed = TRUE))
#plot(g)

# degree
degrees <- degree(g, mode="total")

paste("top 5 vrcholu se stupnem:")
head(sort(degrees, decreasing=TRUE))

plot(degrees,
     xlab = "vrchol (id)",
     ylab = "stupen")

par(mfrow=c(1,2))
plot(sort(degrees, decreasing = F), type="l",
     xlab = "pocet vrcholu",
     ylab = "stupen")
plot(sort(degrees, decreasing = T), type="l",
     xlab = "pocet vrcholu",
     ylab = "stupen")

d = degree(g, mode = "all")
hist(d, breaks = 100, main="Histogram stupňů", xlab="Stupeň", ylab="Počet vrcholů")

# histogram stupnu
d = degree(g, mode = "all")
dd = degree.distribution(g, cumulative = F)
degree = 1:max(d)
probability = dd[-1]
nonzero.position = which(probability != 0)
probability = probability[nonzero.position]
degree = degree[nonzero.position]

deg.dist <- degree_distribution(g, cumulative=T, mode="all")

par(mfrow = c(2, 1), mar=c(4,4,4,4))
plot(dd, type="l",
     xaxt = "n",
     xlab = "Stupen",
     ylab = "Relativni frekvence")

par(mfrow = c(1,1))
plot( x=0:max(d), y=1-deg.dist, pch=19, type="l",
      xlab="Stupen", ylab="Kumulativní frekvence")

degree_n = 100
plot(head(degree_distribution, n = degree_n),
     xaxt = "n",
     xlab = "stupen vrcholu",
     ylab = "relativni cetnost stupne vrcholu",
     main = paste("prvnich", degree_n, "stupnu relativni cetnosti")) +
  axis(1, at = seq(0, degree_n), las = 2)

# number of components
paste("pocet souvislych komponent:", no.clusters(g))

# components sizes
components = clusters(g)

cluster_louvain(g)
#c = cliques(g)
kkore = coreness(g)



##################################

remove_node_with_highest_degree <- function(g){
  max = which.max(degree(g))
  g = delete.vertices(g, max)
  return (g)
}

remove_random_node <- function(g){
  node_count = vcount(g)
  node = sample(1:node_count, 1)
  g = delete.vertices(g, node)
  return (g)
}

remove_random_edge <- function(g) {
  edge_count = ecount(g)
  if (edge_count > 0)
  {
    edge = sample(1:edge_count, 1)
    if (edge > 0)
    {
      g = delete.edges(g, edge)
      g = delete.vertices(simplify(g), degree(g) == 0)
    }
  }
  return (g)
}

odolnost_site <-
  function(g,
           iterations,
           modify_graph_method,
           text_main,
           text_xlab,
           step = 1) {
    cl = clusters(g)
    no_clusters = c(cl$no)
    max_cluster_sizes = max(c(cl$csize))
    average_degrees = c(mean(degree(g)))
    average_distances = c(mean_distance(g, directed = TRUE, unconnected = FALSE))
    x_values = c(0)
    
    x = 1
    for (i in 1:iterations) {
      g = modify_graph_method(g)
      
      if(i %% step == 0)
      {
        c = clusters(g)
        
        no_clusters = append(no_clusters, c$no)
        max_cluster_sizes = append(max_cluster_sizes, max(c$csize))
        average_degrees = append(average_degrees, mean(degree(g)))
        average_distances = append(average_distances,
                                   mean_distance(g, directed = TRUE, unconnected = FALSE))
        
        x_values = append(x_values, x)
      }
      x = x + 1
    }
    
    par(
      mfrow = c(2, 2),
      mar = c(4, 4, 1, 1),
      oma = c(0, 0, 2, 0)
    )
    plot(
      x_values,
      no_clusters,
      type = "l",
      xlab = text_xlab,
      ylab = "pocet shluku"
    )
    plot(
      x_values,
      max_cluster_sizes,
      type = "l",
      xlab = text_xlab,
      ylab = "pocet vrcholu nejvetsiho shluku"
    )
    plot(
      x_values,
      average_degrees,
      type = "l",
      xlab = text_xlab,
      ylab = "prumerny stupen"
    )
    plot(
      x_values,
      average_distances,
      type = "l",
      xlab = text_xlab,
      ylab = "prumerna vzdalenost"
    )
    title(text_main, outer = TRUE)
    
    #result = c(no_clusters, max_cluster_sizes, average_degrees, average_distances)
    #names(result) = c("Number of clusters", "Max cluster size", "Average degree", "Average distance")
    result = list(
      "X" = x_values,
      "NumberOfClusters" = no_clusters,
      "MaxClusterSizes" = max_cluster_sizes,
      "AverageDegrees" = average_degrees,
      "AverageDistances" = average_distances
    )
    return (result)
  }

#g = sample_gnm(n=50, m=100)
#n = 3
#m = 10

n = vcount(g)
m = ecount(g)

odolnost_site_highest_degree_node = odolnost_site(
  g,
  n - 1,
  remove_node_with_highest_degree,
  "Odstranovani vrcholu s nejvetsim stupnem" ,
  "pocet odstranenych vrcholu"
)
odolnost_site_random_node = odolnost_site(
  g,
  n - 1,
  remove_random_node,
  "Odstranovani nahodneho vrcholu ",
  "pocet odstranenych vrcholu"
)
odolnost_site_random_edge = odolnost_site(g,
                                          m - 3000,
                                          remove_random_edge,
                                          "Odstranovani nahodne hrany",
                                          "pocet odstranenych hran",
                                          500)

library(ggplot2)

leg = c("Největší stupeň", "Náhodný")
leg.pos = c(0.85, 0.9)

p1 <- ggplot(
  data = data.frame(
    x = odolnost_site_highest_degree_node$X,
    val = c(
      odolnost_site_highest_degree_node$NumberOfClusters,
      odolnost_site_random_node$NumberOfClusters
    ),
    variable = rep(
      leg,
      each = NROW(odolnost_site_highest_degree_node$X)
    )
  ),
  aes(x = x, y = val) 
  
) + geom_line(aes(colour = variable)) + labs(x = "Počet odstraněných vrcholů", y =
                                               "Počet shluků", color = "") +
  theme_light() + theme(legend.position = leg.pos)

p2 <- ggplot(
  data = data.frame(
    x = odolnost_site_highest_degree_node$X,
    val = c(
      odolnost_site_highest_degree_node$MaxClusterSizes,
      odolnost_site_random_node$MaxClusterSizes
    ),
    variable = rep(
      leg,
      each = NROW(odolnost_site_highest_degree_node$X)
    )
  ),
  aes(x = x, y = val)
  
) + geom_line(aes(colour = variable)) + labs(x = "Počet odstraněných vrcholů", y =
                                               "Velikost největšího shluku", color = "")+
  theme_light() + theme(legend.position = leg.pos)

p3 <- ggplot(
  data = data.frame(
    x = odolnost_site_highest_degree_node$X,
    val = c(
      odolnost_site_highest_degree_node$AverageDegrees,
      odolnost_site_random_node$AverageDegrees
    ),
    variable = rep(
      leg,
      each = NROW(odolnost_site_highest_degree_node$X)
    )
  ),
  aes(x = x, y = val)
  
) + geom_line(aes(colour = variable)) + labs(x = "Počet odstraněných vrcholů", y =
                                               "Průměrný stupeň", color = "")+
  theme_light() + theme(legend.position = leg.pos)

p4 <- ggplot(
  data = data.frame(
    x = odolnost_site_highest_degree_node$X,
    val = c(
      odolnost_site_highest_degree_node$AverageDistances,
      odolnost_site_random_node$AverageDistances
    ),
    variable = rep(
      leg,
      each = NROW(odolnost_site_highest_degree_node$X)
    )
  ),
  aes(x = x, y = val)
  
) + geom_line(aes(colour = variable)) + labs(x = "Počet odstraněných vrcholů", y =
                                               "Průměrná délka cesty", color = "")+
  theme_light() + theme(legend.position = leg.pos)

library(gridExtra)
grid.arrange(p1, p2, p3, p4, ncol = 2, nrow = 2)

library(rworldmap)
par(mfrow = c(1, 1), mar=c(0,0,0,0))
newmap <- getMap(resolution = "low")
plot(newmap, asp = 1)
points(nodes$Longitude, nodes$Latitude, col="red", cex=0.5)

library(maps)
map("world", mar=c(0,0,0,0))
points(nodes$Longitude, nodes$Latitude, col="red", cex=0.5)