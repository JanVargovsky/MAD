hist(table(RandomEdge[,1]))
hist(table(RandomNode[,1]))
hist(table(BA_10_3_5500[,1]))

plot(ecdf(table(RandomEdge[,1])), col="red", main="")
par(new=TRUE)
plot(ecdf(table(RandomNode[,1])), col="blue", main="")
par(new=TRUE)
plot(ecdf(table(BA_10_3_5500[,1])), col="green", main="")

library(lattice)
library(latticeExtra)
a <- data.frame(
  o=ecdf(table(BA_10_3_5500[,1])),
  re=ecdf(table(RandomEdge[,1])),
  rn=ecdf(table(RandomNode[,1])))

library(ggplot2)
re = data.frame(RandomEdge[,1])
colnames(re) <- "Degree"
rn = data.frame(RandomNode[,1])
colnames(rn) <- "Degree"
o = data.frame(BA_10_3_5500[,1])
colnames(o) <- "Degree"

ggplot(re, aes(Degree)) + stat_ecdf(geom = "step")
ggplot(rn, aes(Degree)) + stat_ecdf(geom = "step")
ggplot(o, aes(Degree)) + stat_ecdf(geom = "step")

ggplot(re, rn, aes(Degree)) + stat_ecdf(geom = "step")       
