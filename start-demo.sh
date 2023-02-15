#!/bin/zsh

# Save the current directory and go to the producer directory
pushd . || exit

cd Producer || exit
# Build the Producer Project
dotnet build
# Make the Producer file an executable
chmod +x ./bin/Debug/net7.0/Producer
# run the producer in the background
 ./bin/Debug/net7.0/Producer &
# Go to the Consumer directory
cd ../Consumer || exit
# Build the Consumer Project
dotnet build
# Make the Consumer file an executable
chmod +x ./bin/Debug/net7.0/Consumer
# run two instances of the Consumer
./bin/Debug/net7.0/Consumer &
./bin/Debug/net7.0/Consumer &
# Return to the original directory
popd || exit


