# Date_time
# city,state/province
# country,
# UFO_shape
# length_of_encounter_seconds
# described_duration_of_encounter
# description
# date_documented
# latitude
# longitude

library(ggplot2)
library(dplyr)
library(gridExtra)
library(grid)
library(RColorBrewer)
library(ggrepel)
library(ggthemes)
library(viridis)
library(ggjoy)
library(ggridges)
library(magrittr)
library(dplyr)
library(viridis)
library(RColorBrewer)
library(RgoogleMaps)
library(plotGoogleMaps)

records <- read.csv("../ufo_sighting_data.csv", sep=",", header = TRUE)
records$longitude<-as.numeric(records$longitude)
records$time <- stringr::str_sub(records$Date_time, -5, -1)
records$hour <- stringr::str_sub(records$Date_time, -5, -4)
records[sapply(records, is.character)] <- lapply(records[sapply(records, is.character)], as.factor) # some SO "hack"

trainingSet <- records[records[,"UFO_shape"] != "",]
#trainingSet <- trainingSet[trainingSet[,"UFO_shape"] != "light",] # test only
predictSet <- records[records[,"UFO_shape"] == "",]

#sum(is.na(trainingSet$length_of_encounter_seconds))

#filteredTrainingSet <- trainingSet[c("UFO_shape", "length_of_encounter_seconds", "hour")]
#model <- naiveBayes(UFO_shape ~ length_of_encounter_seconds + hour, trainingSet[c("UFO_shape", "length_of_encounter_seconds", "hour")])
#model <- naiveBayes(UFO_shape ~ length_of_encounter_seconds + hour, trainingSet)
#model <- naiveBayes(UFO_shape ~ city, trainingSet)
model <- naiveBayes(UFO_shape ~ hour + length_of_encounter_seconds + city, trainingSet)
model <- naiveBayes(UFO_shape ~ length_of_encounter_seconds, trainingSet)
predictedShapes <- predict(model, predictSet)
predictSet = cbind(predictedShape = predictedShapes, predictSet)
table(predictedShapes)
plot(predictedShapes)

# sandbox
plot(records$UFO_shape)
plot(records$length_of_encounter_seconds)

rm(model, predicted, predictSet, trainingSet, records)

#analysis
plot(table(records$UFO_shape), type= "h")
qplot(records$UFO_shape) + coord_flip()

records$time <- stringr::str_sub(records$Date_time, -5, -1)
plot(table(records$time), type= "h")
qplot(records$time)

records$hour <- stringr::str_sub(records$Date_time, -5, -4)
plot(table(records$hour), type= "h")
qplot(records$hour) + coord_flip()

records$dateWithoutYear <- stringr::str_sub(records$Date_time, 0, -12)
plot(table(records$dateWithoutYear), type= "h")

records$month <- months(anytime::anydate(records$Date_time))
qplot((records[complete.cases(records$month), ])$month)

records$day <- weekdays(anytime::anydate(records$Date_time))
qplot((records[complete.cases(records$day), ])$day)

pie(table(records$day), col =brewer.pal(7,'Paired'))
# cols <- rainbow(nrow(records));
# pie(records$day,labels=paste0(round(records$freq/sum(records$freq)*100,2),'%'),col=cols);
# legend('bottom',legend=records$inst,pch='■',ncol=nrow(records),bty='n',col=cols);
# records %>% 
#   filter(complete.cases(day)) %>%
#   group_by(day) %>%
#   summarize(count=n()) %>%
#   ggplot(aes(x=factor(day),y=count,group=day)) +
#   scale_color_manual(name="",values=brewer.pal(12,"Paired")) +
#   theme_light()
# days <- records %>% 
#   filter(complete.cases(day))
# 
#   ggplot(data=days, aes(days$day)) + geom_histogram(stat=count(days$day))
# geom_histogram()

#weekdays(as.Date("9/30/2013",))
#strftime("9/30/2013", '%A')
#weekdays(anytime::anydate("9/30/2013"))

map <- getMap(resolution = "low")
plot(map)
points(records$latitude, records$longitude, col="red", cex = .6)

lat <- as.data.frame(records$latitude)
lon <- as.data.frame(records$longitude)
coords <- as.data.frame(cbind(lon = records$longitude, lat = records$latitude))
coordinates(coords) <- ~lat+lon
proj4string(coords) <- CRS("+init=epsg:4326")
plotGoogleMaps(coords)
mapgilbert <- get_map(location = c(lon = mean(lon), lat = mean(lat)), zoom = 4, maptype = "satellite", scale = 2)

countries_map <-map_data("world")
world_map<-ggplot() + 
  geom_map(data = countries_map, 
           map = countries_map,aes(x = lon, y = lat, map_id = region, group = group),
           fill = "white", color = "black", size = 0.1)
world_map + geom_point(data=records,aes(x=longitude,y=latitude),alpha=.5,size=.25) + theme_fivethirtyeight() + ggtitle('Location of UFO sightings')

df<-read.csv("../ufo_sighting_data.csv", sep=",", stringsAsFactors=F)
df$latitude<-as.numeric(df$latitude)
df$year<-sapply(df$Date_time, function(x) as.numeric(strsplit(strsplit(x," ")[[1]][1],"/")[[1]][3]))
df$month<-sapply(df$Date_time, function(x) as.numeric(strsplit(strsplit(x," ")[[1]][1],"/")[[1]][1]))
df$day<-sapply(df$Date_time, function(x) as.numeric(strsplit(strsplit(x," ")[[1]][1],"/")[[1]][2]))
df$hour<-sapply(df$Date_time, function(x) as.numeric(strsplit(strsplit(x," ")[[1]][2],":")[[1]][1]))
df$hour<-ifelse(df$hour==24,0,df$hour)
df$min<-sapply(df$Date_time, function(x) as.numeric(strsplit(strsplit(x," ")[[1]][2],":")[[1]][2]))
df$month_name<-sapply(df$month, function(x) month.name[x])
df$month_name_ordered<-factor(df$month_name, levels =c(month.name))
df$DATE<-sapply(df$Date_time, function(x) strsplit(x,' ')[[1]][1])
df$DATE_2<-as.Date(df$DATE,"%m/%d/%Y")
df$weekday <- factor(weekdays(df$DATE_2, T), levels = rev(c("Mon", "Tue", "Wed", "Thu","Fri", "Sat", "Sun")))
df$length_min<-as.numeric(df$length_of_encounter_seconds)/60

countries_map <-map_data("world")
world_map<-ggplot() + 
  geom_map(data = countries_map, 
           map = countries_map,aes(x = long, y = lat, map_id = region, group = group),
           fill = "white", color = "black", size = 0.1)

world_map + geom_point(data=df,aes(x=longitude,y=latitude),alpha=.5,size=.25, color="red") + theme_light()

qplot(df$Date_time)

df %>% 
  filter(length_min < 50000 && length_min > 0) %>%
  group_by(UFO_shape, length_min) %>%
  summarize(length_in_min=n(), mean=mean(length_min)) %>%
  ggplot(aes(x=factor(UFO_shape),y=length_in_min,group=length_min)) +
    coord_flip() +
    scale_color_manual(name="",values=brewer.pal(12,"Paired")) +
    geom_point(color="black",size=.5,alpha=1) +
    theme_light()

