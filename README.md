# Tetris AI
Training AI to play Tetris using Unity ML-Agents. The SAC training algorithm was used for training, other hyperparameters are in config.yaml. Current high score is 460260.

## Training
Run mlagents from the command line and play the ```Train.unity``` scene.
```mlagents-learn config/trainer_config.yaml --run-id TetrisLearning```
View the results using tensorboard.
```tensorboard --logdir=summaries --port=6006 --bind_all```

## Testing
Test the generated model in the ```Play.unity``` scene, simply add the .nn file in the model field of behaviour parameters and check the Behaviour Type is set to Inference.

## References
https://github.com/Unity-Technologies/ml-agents/blob/release_3_distributed/docs/Training-Configuration-File.md
https://codemyroad.wordpress.com/2013/04/14/tetris-ai-the-near-perfect-player/
