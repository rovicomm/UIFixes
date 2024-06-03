﻿using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RootMotion.FinalIK.InteractionTrigger.Range;

namespace UIFixes
{
    public class InsuranceInteractions(Item item, ItemUiContext uiContext, int playerRubles) : ItemInfoInteractionsAbstractClass<InsuranceInteractions.EInsurers>(uiContext)
    {
        private readonly InsuranceCompanyClass insurance = uiContext.Session.InsuranceCompany;
        private readonly Item item = item;
        private readonly int playerRubles = playerRubles;
        private List<ItemClass> items;
        private readonly Dictionary<string, int> prices = [];

        public void LoadAsync(Action callback)
        {
            ItemClass itemClass = ItemClass.FindOrCreate(item);
            items = insurance.GetItemChildren(itemClass).Flatten(insurance.GetItemChildren).Concat([itemClass])
                .Where(i => insurance.ItemTypeAvailableForInsurance(i) && !insurance.InsuredItems.Contains(i))
                .ToList();

            insurance.GetInsurePriceAsync(items, _ =>
            {
                foreach (var insurer in insurance.Insurers)
                {
                    int price = this.items.Select(i => insurance.InsureSummary[insurer.Id][i]).Where(s => s.Loaded).Sum(s => s.Amount);
                    prices[insurer.Id] = price;

                    string priceColor = price > playerRubles ? "#FF0000" : "#ADB8BC";

                    string text = string.Format("<b><color=#C6C4B2>{0}</color> <color={1}>({2} ₽)</color></b>", insurer.LocalizedName, priceColor, price);

                    base.method_2(MakeInteractionId(insurer.Id), text, () => this.Insure(insurer.Id));
                }

                callback();
            });
        }

        private void Insure(string insurerId)
        {
            insurance.SelectedInsurerId = insurerId;
            insurance.InsureItems(this.items, result => { });
        }

        public IResult GetButtonInteraction(string interactionId)
        {
            string traderId = interactionId.Split(':')[1];
            if (prices[traderId] > playerRubles)
            {
                return new FailedResult("ragfair/Not enough money", 0);
            }

            return SuccessfulResult.New;
        }

        public override void ExecuteInteractionInternal(EInsurers interaction)
        {
        }

        public override bool IsActive(EInsurers button)
        {
            return button == EInsurers.None && !this.insurance.Insurers.Any();
        }

        public override IResult IsInteractive(EInsurers button)
        {
            return new FailedResult("No insurers??", 0);
        }

        public override bool HasIcons
        {
            get { return false; }
        }

        public enum EInsurers
        {
            None
        }
        private static string MakeInteractionId(string traderId)
        {
            return "UIFixesInsurerId:" + traderId;
        }

        public static bool IsInsuranceInteractionId(string id)
        {
            return id.StartsWith("UIFixesInsurerId:");
        }
    }

    public static class InsuranceExtensions
    {
        public static bool IsInsuranceInteraction(this DynamicInteractionClass interaction)
        {
            return interaction.Id.StartsWith("UIFixesInsurerId:");
        }
    }
}
