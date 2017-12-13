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
# time - mapping
# hour - mapping

records <- read.csv("../ufo_sighting_data.csv", sep=",", header = TRUE)
#records$time <- stringr::str_sub(records$Date_time, -5, -1)
#records$hour <- stringr::str_sub(records$Date_time, -5, -4)

trainingSet <- records[records[,"UFO_shape"] != "",]
trainingSet <- trainingSet[trainingSet[,"UFO_shape"] != "light",] ## test only
predictSet <- records[records[,"UFO_shape"] == "",]

#sum(is.na(trainingSet$length_of_encounter_seconds))

model <- naiveBayes(UFO_shape ~ ., trainingSet)
#model <- naiveBayes(UFO_shape ~ hour + length_of_encounter_seconds + city, trainingSet) wtf is wrong with city?!
3model <- naiveBayes(UFO_shape ~ length_of_encounter_seconds, trainingSet)
predictedShapes <- predict(model, predictSet)
predictSet = cbind(predictedShape = predictedShapes, predictSet)
table(predictedShapes)
plot(predictedShapes)

# sandbox
plot(records$UFO_shape)

rm(model, predicted, predictSet, trainingSet, records)
