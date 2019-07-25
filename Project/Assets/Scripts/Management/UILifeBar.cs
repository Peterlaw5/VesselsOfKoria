using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace VoK
{
    public class UILifeBar : MonoBehaviour
    {
        public GameObject lifeBarPiece;
        public Transform lifeBarTransform;
        public Image lifeBarFill;
        public float singlePieceValue = 50f;
        public float piecesStartOffset = -57f;
        public float piecesStartOffsetIncrease = 3f;
        public void SetupLifebar(GamePlayer player, Color color)
        {
            if (lifeBarTransform.childCount > 0) return; 
            StartCoroutine(CoSetupLifebar(player, color));
        }
        IEnumerator CoSetupLifebar(GamePlayer player, Color color)
        {
            yield return new WaitUntil(() => player.Seeker != null);
            float life = player.Seeker.maxHealth;
            float spacing = piecesStartOffset;
            while (life > singlePieceValue * 0.5f)
            {
                life -= singlePieceValue;
                Instantiate(lifeBarPiece, lifeBarTransform);
                spacing += piecesStartOffsetIncrease;
            }
            lifeBarTransform.GetComponent<HorizontalLayoutGroup>().spacing = spacing;
            SetLife(1f);
            SetLifeColor(color);
        }
        public void SetLife(float amount)
        {
            lifeBarFill.fillAmount = amount;
        }
        public void SetLifeColor(Color color)
        {
            lifeBarFill.color = color;
        }
    }
}
