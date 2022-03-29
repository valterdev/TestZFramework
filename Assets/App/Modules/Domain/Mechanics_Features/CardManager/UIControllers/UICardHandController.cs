using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ZFramework;
using System.Linq;

public class UICardHandController : IUIController
{
    public void Render(VisualElement page)
    {
        int maxCardsInHand = App.CardManager.CardsInArm.Count;

        for (int i = 0; i < maxCardsInHand; i++)
        {
            var cardDBInfo = App.CardManager.GetCardInfoByID(App.CardManager.CardsInArm[i].ID);
            var cardView = App.UI.GetUIComponentInstance("CardView");

            cardView.Q<VisualElement>("skill_image").style.backgroundImage = cardDBInfo.Img;
            cardView.Q<Label>("mana").text = App.CardManager.CardsInArm[i].Effects.Where(x => x.Key == "mana_cost").First().Value.ToString();
            cardView.Q<Label>("title").text = cardDBInfo.NameKey;
            cardView.Q<Label>("descr").text = cardDBInfo.DescrKey;

            if (i != maxCardsInHand / 2)
            {
                cardView.style.transformOrigin = new TransformOrigin(i < maxCardsInHand / 2 ? (maxCardsInHand / 2 - i) * 100f : (i - maxCardsInHand / 2 - 1) * -100f, Length.Percent(50), 0);
                cardView.style.rotate = new StyleRotate(new Rotate(new Angle(i < maxCardsInHand / 2 ? (maxCardsInHand / 2 - i) * -5f : (i - 2) * 5f, AngleUnit.Degree)));

                cardView.style.transitionDelay = new List<TimeValue>() { new TimeValue(0, TimeUnit.Millisecond) };
                cardView.style.transitionProperty = new List<StylePropertyName>() { new StylePropertyName("rotate") };
                cardView.style.transitionDuration = new List<TimeValue>() { new TimeValue(10, TimeUnit.Second) };

            }
            //DOTween.To(() => 0f, x => cardView.style.rotate = new StyleRotate(new Rotate(new Angle(i < maxCardsInHand / 2 ? (maxCardsInHand / 2 - i) * -5f : (i - 2) * 5f, AngleUnit.Degree))), 1f, 2f).SetEase(Ease.Linear);

            page.Q<VisualElement>("hand").Add(cardView);
        }
    }
}
