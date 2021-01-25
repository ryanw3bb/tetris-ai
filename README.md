# Tetris AI
Training AI to play Tetris using Unity's Reinforcement Learning package ML-Agents. The SAC training algorithm was used for training with other hyperparameters defined in trainer_config.yaml. Top recorded high score is 460260.

## Demo
![gif](tetris.gif)

## Dependencies
This project was created using Unity 2019.4.0 LTS and [ML-Agents release 2](https://github.com/Unity-Technologies/ml-agents/releases/tag/release_2). Python 3.7 and the ml-agents 0.16.1 Python package is required for training a new model.

## Training
Launch mlagents-learn from the command line and run the ```Train.unity``` scene.

```mlagents-learn config/trainer_config.yaml --run-id TetrisLearning```

View the results using tensorboard.

```tensorboard --logdir=summaries --port=6006 --bind_all```

## Testing
Test the generated model in the ```Play.unity``` scene, simply add the .nn file in the model field of behaviour parameters and check the Behaviour Type is set to Inference.

## References
https://github.com/Unity-Technologies/ml-agents/blob/release_2/docs/Readme.md   
https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified/docs/Training-Configuration-File.md  
https://codemyroad.wordpress.com/2013/04/14/tetris-ai-the-near-perfect-player/
