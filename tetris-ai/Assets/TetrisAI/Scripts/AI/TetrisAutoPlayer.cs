using UnityEngine;

public class TetrisAutoPlayer : TetrisAgent
{
    bool firstPiece = true;

    public override void OnActionReceived(float[] vectorAction)
    {
        int currentAction = -1;
        float highestTotal = float.MinValue;

        for (int i = 0; i < controller.States.Count; i += 4)
        {
            int action = Mathf.FloorToInt((i / 4) / 40f);

            if (!controller.MaskedActions.Contains(action))
            {
                float lines = 0.760666f * controller.States[i];
                float height = -0.510066f * controller.States[i + 1];
                float bumpiness = -0.184483f * controller.States[i + 2];
                float holes = -0.35663f * controller.States[i + 3];
                float total = lines + height + bumpiness + holes;

                if (total > highestTotal)
                {
                    highestTotal = total;
                    currentAction = action;
                }
            }
        }

        // currentAction will always be -1 on first go
        if (firstPiece)
        {
            firstPiece = false;
            currentAction = 16;
        }

        if (!controller.MaskedActions.Contains(currentAction))
        {
            // horizontal (x) position & rotation
            // 40 possible values
            int rotate = Mathf.RoundToInt(currentAction % TetrisSettings.NumRotations);
            int position = Mathf.FloorToInt(currentAction / TetrisSettings.NumRotations);

            Debug.Log("action is " + currentAction + " " + highestTotal + " " + rotate + " " + position);

            controller.CreateBlock(position, TetrisSettings.Rotations[rotate]);

            //controller.CurrentBlock.TransformPosition(position, TetrisSettings.Rotations[rotate], true);
        }
        else
        {
            controller.GameOver();
        } 
    }
}
