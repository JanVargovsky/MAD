
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

world_map + geom_point(data=df,aes(x=longitude,y=latitude),alpha=.5,size=.25, color="red") + theme_fivethirtyeight()

df %>% 
  group_by(hour,month_name_ordered) %>%
  summarize(count=n()) %>%
  ggplot(aes(x=factor(hour),y=count,color=month_name_ordered,group=month_name_ordered)) + geom_line() + scale_color_manual(name="",values=brewer.pal(12,"Paired")) + geom_point(color='black',size=.5,alpha=1) + theme_light()
