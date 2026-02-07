using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Interfaces;
using ABH.Services.Logic.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.Services.Logic
{
	public class RequirementOperationServiceRealImpl : IRequirementOperationService
	{
		private Dictionary<Type, Dictionary<RequirementType, Func<object, Requirement, bool>>> CheckFunctionsByRequirement = new Dictionary<Type, Dictionary<RequirementType, Func<object, Requirement, bool>>>();

		public void InitializeRequirementOperations()
		{
			CheckFunctionsByRequirement.Clear();
			var dictionary = new Dictionary<RequirementType, Func<object, Requirement, bool>>();
			CheckFunctionsByRequirement.Add(typeof(PlayerGameData), dictionary);
			dictionary.Add(RequirementType.Level, delegate(object o, Requirement r)
			{
				var playerGameData26 = o as PlayerGameData;
				return r.NameId == "max" ? (float)playerGameData26.Data.Level < r.Value : (float)playerGameData26.Data.Level >= r.Value;
			});
			dictionary.Add(RequirementType.HaveMasteryFactor, (o, r) => EvaluateMasteryFactorRequirement(o, r));
			dictionary.Add(RequirementType.NotHaveMasteryFactor, (o, r) => !EvaluateMasteryFactorRequirement(o, r));
			dictionary.Add(RequirementType.TimeSinceLastPurchase, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				if (player.Data.TimeStampOfLastPurchase != null)
				{
					return DIContainerLogic.GetTimingService().GetCurrentTimestamp() - player.Data.TimeStampOfLastPurchase >= r.Value;
				}
				return false;
			});
			dictionary.Add(RequirementType.HighestLeagueReached, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				if (player.CurrentPvPSeasonGameData != null)
				{
					return player.Data.HighestFinishedLeague >= r.Value;
				}
				return false;
			});
			dictionary.Add(RequirementType.BirdMasteryFactorMinimum, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				return player.InventoryGameData.Items[InventoryItemType.Class]
					.Where(c => (c as ClassItemGameData).BalancingData.RestrictedBirdId == r.NameId)
					.ToList()
					.Any(c => (c as ClassItemGameData).Data.Level < r.Value);
			});
			dictionary.Add(RequirementType.BirdMasteryFactorMaximum, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				return player.InventoryGameData.Items[InventoryItemType.Class]
					.Where(c => (c as ClassItemGameData).BalancingData.RestrictedBirdId == r.NameId)
					.ToList()
					.Any(c => (c as ClassItemGameData).Data.Level > r.Value);
			});
			dictionary.Add(RequirementType.UsedFriends, delegate(object o, Requirement r)
			{
				var playerGameData25 = o as PlayerGameData;
				var value = new List<string>();
				return playerGameData25.Data.SocialEnvironment.FriendShipGateUnlocks.TryGetValue(r.NameId, out value) && (float)value.Count >= r.Value;
			});
			dictionary.Add(RequirementType.NotUseBirdInBattle, delegate(object o, Requirement r)
			{
				var playerGameData24 = o as PlayerGameData;
				for (var j = 0; j < playerGameData24.Data.SelectedBirdIndices.Count; j++)
				{
					var nameId2 = playerGameData24.Birds[playerGameData24.Data.SelectedBirdIndices[j]].BalancingData.NameId;
					if (nameId2 == r.NameId)
					{
						return false;
					}
				}
				return true;
			});
			dictionary.Add(RequirementType.UseBirdInBattle, delegate(object o, Requirement r)
			{
				var playerGameData23 = o as PlayerGameData;
				for (var i = 0; i < playerGameData23.Data.SelectedBirdIndices.Count; i++)
				{
					var nameId = playerGameData23.Birds[playerGameData23.Data.SelectedBirdIndices[i]].BalancingData.NameId;
					if (nameId == r.NameId)
					{
						return true;
					}
				}
				return false;
			});
			dictionary.Add(RequirementType.LostPvpBattle, delegate(object o, Requirement r)
			{
				var playerGameData22 = o as PlayerGameData;
				return playerGameData22.Data.LostAnyPvpBattle;
			});
			dictionary.Add(RequirementType.HaveEventScore, delegate(object o, Requirement r)
			{
				var playerGameData21 = o as PlayerGameData;
				return playerGameData21.CurrentEventManagerGameData != null && (float)playerGameData21.CurrentEventManagerGameData.Data.CurrentScore >= r.Value;
			});
			dictionary.Add(RequirementType.TotalMoneySpent, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				return player.Data.TotalDollarsSpent >= r.Value;
			});
			dictionary.Add(RequirementType.HaveCurrentHotpsotState, delegate(object o, Requirement r)
			{
				var playerGameData18 = o as PlayerGameData;
				return playerGameData18.WorldGameData.CurrentHotspotGameData != null && playerGameData18.WorldGameData.CurrentHotspotGameData.Data.UnlockState.ToString().ToLower() == r.NameId.ToLower() ? true : false;
			});
			dictionary.Add(RequirementType.LostUnresolvedHotspot, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				if (player.Data.UnresolvedHotspotsLost != null && player.Data.UnresolvedHotspotsLost.ContainsKey(r.NameId))
				{
					return r.Value <= player.Data.UnresolvedHotspotsLost[r.NameId];
				}
				return false;
			});
			dictionary.Add(RequirementType.HaveCurrentChronicleCaveState, delegate(object o, Requirement r)
			{
				var playerGameData17 = o as PlayerGameData;
				return playerGameData17.ChronicleCaveGameData.CurrentHotspotGameData != null && playerGameData17.ChronicleCaveGameData.CurrentHotspotGameData.Data.UnlockState.ToString().ToLower() == r.NameId.ToLower() ? true : false;
			});
			dictionary.Add(RequirementType.TutorialCompleted, delegate(object o, Requirement r)
			{
				var playerGameData16 = o as PlayerGameData;
				return playerGameData16.Data.TutorialTracks != null && playerGameData16.Data.TutorialTracks.ContainsKey(r.NameId) && (float)playerGameData16.Data.TutorialTracks[r.NameId] == r.Value;
			});
			dictionary.Add(RequirementType.HaveEventCampaignHotspotState, delegate(object o, Requirement r)
			{
				var playerGameData15 = o as PlayerGameData;
				return DIContainerLogic.EventSystemService.IsCurrentEventAvailable(playerGameData15) && playerGameData15.CurrentEventManagerGameData.CurrentMiniCampaign != null && playerGameData15.CurrentEventManagerGameData.CurrentMiniCampaign.CurrentHotspotGameData != null && playerGameData15.CurrentEventManagerGameData.CurrentMiniCampaign.CurrentHotspotGameData.Data.UnlockState.ToString().ToLower() == r.NameId.ToLower() ? true : false;
			});
			dictionary.Add(RequirementType.HaveUnlockedHotpsot, delegate(object o, Requirement r)
			{
				var playerGameData14 = o as PlayerGameData;
				return playerGameData14.WorldGameData.HotspotGameDatas.ContainsKey(r.NameId) && playerGameData14.WorldGameData.HotspotGameDatas[r.NameId].Data.UnlockState >= HotspotUnlockState.ResolvedNew ? true : false;
			});
			dictionary.Add(RequirementType.NotHaveUnlockedHotpsot, delegate(object o, Requirement r)
			{
				var playerGameData13 = o as PlayerGameData;
				return playerGameData13.WorldGameData.HotspotGameDatas.ContainsKey(r.NameId) && playerGameData13.WorldGameData.HotspotGameDatas[r.NameId].Data.UnlockState < HotspotUnlockState.ResolvedNew ? true : false;
			});
			dictionary.Add(RequirementType.HaveBird, delegate(object o, Requirement r)
			{
				var playerGameData12 = o as PlayerGameData;
				var bird = playerGameData12.GetBird(r.NameId);
				return r.Value == 0f ? playerGameData12.GetBird(r.NameId) == null : playerGameData12.GetBird(r.NameId) != null;
			});
			dictionary.Add(RequirementType.HaveBirdCount, delegate(object o, Requirement r)
			{
				var playerGameData11 = o as PlayerGameData;
				var count = playerGameData11.Birds.Count;
				if (r.NameId.Contains("g"))
				{
					return r.Value > (float)count;
				}
				return r.NameId.Contains("e") ? Math.Abs(r.Value - (float)count) < 0.5f : r.Value <= (float)count;
			});
			dictionary.Add(RequirementType.HaveItem, delegate(object o, Requirement r)
			{
				var playerGameData10 = o as PlayerGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(playerGameData10.InventoryGameData, r.NameId) >= r.Value;
			});
			dictionary.Add(RequirementType.NotHaveClass, delegate(object o, Requirement r)
			{
				var playerGameData9 = o as PlayerGameData;
				return !DIContainerLogic.InventoryService.CheckForItem(playerGameData9.InventoryGameData, r.NameId);
			});
			dictionary.Add(RequirementType.HaveClass, delegate(object o, Requirement r)
			{
				var playerGameData8 = o as PlayerGameData;
				return DIContainerLogic.InventoryService.CheckForItem(playerGameData8.InventoryGameData, r.NameId);
			});
			dictionary.Add(RequirementType.DeclinedOffer, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				return player.Data.OffersEndedWithoutPurchase != null && player.Data.OffersEndedWithoutPurchase.Contains(r.NameId);
			});
			dictionary.Add(RequirementType.EndedOffer, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				return player.Data.OffersEnded != null && player.Data.OffersEnded.Contains(r.NameId);
			});
			dictionary.Add(RequirementType.NotHaveItem, delegate(object o, Requirement r)
			{
				var playerGameData7 = o as PlayerGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(playerGameData7.InventoryGameData, r.NameId) < r.Value;
			});
			dictionary.Add(RequirementType.AcceptedOffer, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				var offerEnded = player.Data.OffersEnded != null && player.Data.OffersEnded.Contains(r.NameId);
				var offerEndedWithoutPurchase = player.Data.OffersEndedWithoutPurchase != null && player.Data.OffersEndedWithoutPurchase.Contains(r.NameId);
				if (r.Value == 1)
					return offerEnded && !offerEndedWithoutPurchase;
				else
					return offerEndedWithoutPurchase || !offerEnded;
			});
			dictionary.Add(RequirementType.UnlockedAllClasses, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				
				if (r.NameId == "bird_all")
				{
					var allClassesCount = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>().Count(c => !string.IsNullOrEmpty(c.RestrictedBirdId));
					var ownedClassesCount = player.InventoryGameData.Items[InventoryItemType.Class].Count;
					
					if (r.Value == 0)
						return ownedClassesCount < allClassesCount;
					
					return ownedClassesCount >= allClassesCount;
				}
				
				var birdClassesCount = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>().Count(c => c.RestrictedBirdId == r.NameId && !string.IsNullOrEmpty(c.RestrictedBirdId));
				var ownedBirdClassesCount = player.InventoryGameData.Items[InventoryItemType.Class].Count(c => (c as ClassItemGameData).BalancingData.RestrictedBirdId == r.NameId);
					
				if (r.Value == 0)
					return ownedBirdClassesCount < birdClassesCount;
				
				return ownedBirdClassesCount >= birdClassesCount;
			});
			dictionary.Add(RequirementType.UnlockedAllSkins, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				var saleBundleSkins = DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>().Where(b => b.PartOfSaleBundles).ToList();
				
				if (r.NameId != "bird_all")
				{
					if (r.NameId.Contains("bird"))
					{
						var skinsOfThisBird = new List<string>();
						var birdClasses = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>().Where(c => c.RestrictedBirdId == r.NameId);
						foreach (var classBalancing in birdClasses)
						{
							skinsOfThisBird.Add(classBalancing.NameId);
						}
						var birdSkinsInSales = saleBundleSkins.Count(c => skinsOfThisBird.Contains(c.OriginalClass) && c.PartOfSaleBundles);
						var ownedBirdSkinsInSales = player.InventoryGameData.Items[InventoryItemType.Skin].Count(c =>
							(c as SkinItemGameData).GetOriginalClassBalancingData().RestrictedBirdId == r.NameId &&
							(c as SkinItemGameData).BalancingData.PartOfSaleBundles);
						if (r.Value == 0)
							return ownedBirdSkinsInSales < birdSkinsInSales;

						return ownedBirdSkinsInSales >= birdSkinsInSales;
					}
					else
					{
						var allSkinsOfClassInSales = saleBundleSkins.Count(c => c.OriginalClass == r.NameId);
						var ownedSkinsOfClassInSales = player.InventoryGameData.Items[InventoryItemType.Skin].Count(c =>
							(c as SkinItemGameData).BalancingData.OriginalClass == r.NameId &&
							(c as SkinItemGameData).BalancingData.PartOfSaleBundles);
						if (r.Value == 0)
							return ownedSkinsOfClassInSales < allSkinsOfClassInSales;

						return ownedSkinsOfClassInSales >= allSkinsOfClassInSales;
					}
				}
				var allBirdSkinsInSales = saleBundleSkins.Count;
				var ownedBirdSkinsInSaleBundles = player.InventoryGameData.Items[InventoryItemType.Skin].Count(b => (b as SkinItemGameData).BalancingData.PartOfSaleBundles);
				if (allBirdSkinsInSales - ownedBirdSkinsInSaleBundles == 5)
					ownedBirdSkinsInSaleBundles = allBirdSkinsInSales;
				if (r.Value == 0)
					return ownedBirdSkinsInSaleBundles < allBirdSkinsInSales;

				return ownedBirdSkinsInSaleBundles >= allBirdSkinsInSales;
			});
			dictionary.Add(RequirementType.HaveItemWithLevel, delegate(object o, Requirement r)
			{
				var playerGameData6 = o as PlayerGameData;
				IInventoryItemGameData data2 = null;
				return DIContainerLogic.InventoryService.TryGetItemGameData(playerGameData6.InventoryGameData, r.NameId, out data2) && (float)data2.ItemData.Level >= r.Value;
			});
			dictionary.Add(RequirementType.NotHaveItemWithLevel, delegate(object o, Requirement r)
			{
				var playerGameData5 = o as PlayerGameData;
				IInventoryItemGameData data = null;
				return !DIContainerLogic.InventoryService.TryGetItemGameData(playerGameData5.InventoryGameData, r.NameId, out data) || (float)data.ItemData.Level < r.Value;
			});
			dictionary.Add(RequirementType.PayItem, delegate(object o, Requirement r)
			{
				var playerGameData4 = o as PlayerGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(playerGameData4.InventoryGameData, r.NameId) >= r.Value;
			});
			DateTime trustedTime2;
			dictionary.Add(RequirementType.IsSpecificWeekday, (o, r) => DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime2) && trustedTime2.DayOfWeek == (DayOfWeek)(int)Enum.Parse(typeof(DayOfWeek), r.NameId, true));
			DateTime trustedTime;
			dictionary.Add(RequirementType.IsNotSpecificWeekday, (o, r) => DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime) && trustedTime.DayOfWeek != (DayOfWeek)(int)Enum.Parse(typeof(DayOfWeek), r.NameId, true));
			dictionary.Add(RequirementType.HaveLessThan, delegate(object o, Requirement r)
			{
				var playerGameData3 = o as PlayerGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(playerGameData3.InventoryGameData, r.NameId) <= r.Value;
			});
			dictionary.Add(RequirementType.IsConverted, delegate(object o, Requirement r)
			{
				var playerGameData2 = o as PlayerGameData;
				return r.Value == 1f ? playerGameData2.Data.IsUserConverted : !playerGameData2.Data.IsUserConverted;
			});
			dictionary.Add(RequirementType.HaveAllUpgrades, delegate(object o, Requirement r)
			{
				var player2 = o as PlayerGameData;
				return HaveAllUpgrades(r, player2);
			});
			dictionary.Add(RequirementType.NotHaveAllUpgrades, delegate(object o, Requirement r)
			{
				var player = o as PlayerGameData;
				return !HaveAllUpgrades(r, player);
			});
			dictionary.Add(RequirementType.HaveTotalItemsInCollection, delegate(object o, Requirement r)
			{
				var playerGameData = o as PlayerGameData;
				if (playerGameData.Data.CollectiblesPerEvent != null && playerGameData.Data.CollectiblesPerEvent.ContainsKey(r.NameId))
				{
					return (float)playerGameData.Data.CollectiblesPerEvent[r.NameId] >= r.Value;
				}
				if (playerGameData.CurrentEventManagerGameData != null && playerGameData.CurrentEventManagerGameData.Balancing != null && playerGameData.CurrentEventManagerGameData.Balancing.NameId == r.NameId)
				{
					var num = 0;
					foreach (var item in playerGameData.InventoryGameData.Items[InventoryItemType.CollectionComponent])
					{
						if (item.Name != "collection_event_stars")
						{
							num += item.ItemValue;
						}
					}
					return (float)num >= r.Value;
				}
				return false;
			});
			var dictionary2 = new Dictionary<RequirementType, Func<object, Requirement, bool>>();
			CheckFunctionsByRequirement.Add(typeof(InventoryGameData), dictionary2);
			dictionary2.Add(RequirementType.HaveItem, delegate(object o, Requirement r)
			{
				var inventory5 = o as InventoryGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(inventory5, r.NameId) >= r.Value;
			});
			dictionary2.Add(RequirementType.NotHaveItem, delegate(object o, Requirement r)
			{
				var inventory4 = o as InventoryGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(inventory4, r.NameId) < r.Value;
			});
			dictionary2.Add(RequirementType.NotHaveClass, delegate(object o, Requirement r)
			{
				var inventory3 = o as InventoryGameData;
				return !DIContainerLogic.InventoryService.CheckForItem(inventory3, r.NameId);
			});
			dictionary2.Add(RequirementType.HaveClass, delegate(object o, Requirement r)
			{
				var inventory2 = o as InventoryGameData;
				return DIContainerLogic.InventoryService.CheckForItem(inventory2, r.NameId);
			});
			dictionary2.Add(RequirementType.PayItem, delegate(object o, Requirement r)
			{
				var inventory = o as InventoryGameData;
				return (float)DIContainerLogic.InventoryService.GetItemValue(inventory, r.NameId) >= r.Value;
			});
		}

		private static bool EvaluateMasteryFactorRequirement(object o, Requirement r)
		{
			var player = o as PlayerGameData;
			if (r.NameId == "maxmastery")
			{
				return (float)GetMaxMasteryValue(player) >= r.Value;
			}
			if (r.NameId == "avg")
			{
				return GetAverageMasteryValue(player) >= (double)r.Value;
			}
			if (r.NameId == "highavg")
			{
				var highAverageMasteryValue = GetHighAverageMasteryValue(player);
				return highAverageMasteryValue >= r.Value;
			}
			var averageHighest5MasteryValue = GetAverageHighest5MasteryValue(player);
			return averageHighest5MasteryValue >= (double)r.Value;
		}

		public static double GetAverageHighest5MasteryValue(PlayerGameData player)
		{
			var list = player.InventoryGameData.Items[InventoryItemType.Class].OrderBy(c => c.ItemData.Level).ToList();
			var list2 = new List<IInventoryItemGameData>();
			for (var i = 0; i < list.Count; i++)
			{
				if (i > list.Count - 6)
				{
					list2.Add(list[i]);
				}
			}
			return list2.Average(c => c.ItemData.Level);
		}

		public static float GetHighAverageMasteryValue(PlayerGameData player)
		{
			var num = Mathf.FloorToInt((float)player.InventoryGameData.Items[InventoryItemType.Class].Average(c => c.ItemData.Level));
			var num2 = 0f;
			for (var i = 0; i < player.InventoryGameData.Items[InventoryItemType.Class].Count; i++)
			{
				var inventoryItemGameData = player.InventoryGameData.Items[InventoryItemType.Class][i];
				num2 += Mathf.Pow(inventoryItemGameData.ItemData.Level - num, 2f);
			}
			num2 = Mathf.Sqrt(num2 / (float)player.InventoryGameData.Items[InventoryItemType.Class].Count);
			return (float)num + num2 / 2f;
		}

		public static double GetAverageMasteryValue(PlayerGameData player)
		{
			return player.InventoryGameData.Items[InventoryItemType.Class].Average(c => c.ItemData.Level);
		}

		public static int GetMaxMasteryValue(PlayerGameData player)
		{
			return player.InventoryGameData.Items[InventoryItemType.Class].Max(c => c.ItemData.Level);
		}

		private static bool HaveAllUpgrades(Requirement r, PlayerGameData player)
		{
			if (!string.IsNullOrEmpty(r.NameId) && r.NameId.ToLower() == "all")
			{
				for (var i = 0; i < player.InventoryGameData.Items[InventoryItemType.Class].Count; i++)
				{
					var inventoryItemGameData = player.InventoryGameData.Items[InventoryItemType.Class][i];
					if (inventoryItemGameData.ItemData.Level < r.Value)
					{
						return false;
					}
				}
				return true;
			}
			var list = new List<ClassItemGameData>();
			for (var j = 0; j < player.InventoryGameData.Items[InventoryItemType.Class].Count; j++)
			{
				var inventoryItemGameData2 = player.InventoryGameData.Items[InventoryItemType.Class][j];
				list.Add(inventoryItemGameData2 as ClassItemGameData);
			}
			for (var k = 0; k < list.Count; k++)
			{
				var classItemGameData = list[k];
				if (classItemGameData != null && classItemGameData.BalancingData.RestrictedBirdId == r.NameId && classItemGameData.ItemData.Level < r.Value)
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckGenericRequirements(object owner, List<Requirement> requirementsToCheck)
		{
			var failedRequirements = new List<Requirement>();
			return CheckGenericRequirements(owner, requirementsToCheck, out failedRequirements);
		}

		public List<Requirement> AddRequirement(object owner, List<Requirement> requirementsBefore, Requirement requirementToAdd)
		{
			var list = new List<Requirement>(requirementsBefore);
			list.Add(requirementToAdd);
			return list;
		}

		public List<Requirement> GetRequirementDelta(object owner, List<Requirement> requirementsBefore, Requirement requirementToRemove)
		{
			var list = new List<Requirement>();
			for (var i = 0; i < requirementsBefore.Count; i++)
			{
				var requirement = requirementsBefore[i];
				list.Add(new Requirement
				{
					Value = requirement.Value,
					NameId = requirement.NameId,
					RequirementType = requirement.RequirementType
				});
			}
			var requirement2 = list.FirstOrDefault(r => r.RequirementType == requirementToRemove.RequirementType && r.NameId == requirementToRemove.NameId);
			if (requirement2 != null)
			{
				GetRequirementValueDelta(owner, requirement2);
			}
			return list;
		}

		public Requirement GetRequirementDelta(object owner, Requirement requirementBefore)
		{
			var requirement = new Requirement();
			requirement.NameId = requirementBefore.NameId;
			requirement.Value = requirementBefore.Value;
			requirement.RequirementType = requirementBefore.RequirementType;
			var requirement2 = requirement;
			if (requirement2 != null)
			{
				GetRequirementValueDelta(owner, requirement2);
			}
			return requirement2;
		}

		private void GetRequirementValueDelta(object owner, Requirement toModify)
		{
			InventoryGameData inventoryGameData = null;
			if (owner is InventoryGameData)
			{
				inventoryGameData = owner as InventoryGameData;
				switch (toModify.RequirementType)
				{
				case RequirementType.HaveItem:
					toModify.Value = Mathf.Max(toModify.Value - (float)DIContainerLogic.InventoryService.GetItemValue(inventoryGameData, toModify.NameId), 0f);
					break;
				case RequirementType.PayItem:
					toModify.Value = Mathf.Max(toModify.Value - (float)DIContainerLogic.InventoryService.GetItemValue(inventoryGameData, toModify.NameId), 0f);
					break;
				}
			}
		}

		public bool CheckGenericRequirements(object owner, List<Requirement> requirementsToCheck, out List<Requirement> failedRequirements)
		{
			failedRequirements = new List<Requirement>();
			if (requirementsToCheck == null)
			{
				return true;
			}
			for (var i = 0; i < requirementsToCheck.Count; i++)
			{
				var requirement = requirementsToCheck[i];
				if (!CheckRequirement(owner, requirement))
				{
					failedRequirements.Add(requirement);
					return false;
				}
			}
			return true;
		}

		public bool CheckFailRequirements(object owner, List<Requirement> failConditionRequirements)
		{
			if (failConditionRequirements == null || failConditionRequirements.Count == 0)
			{
				return false;
			}
			return !CheckGenericRequirements(owner, failConditionRequirements);
		}

		public bool CheckRequirement(object owner, Requirement req)
		{
			Func<object, Requirement, bool> value = null;
			if (CheckFunctionsByRequirement.ContainsKey(owner.GetType()) && CheckFunctionsByRequirement[owner.GetType()].TryGetValue(req.RequirementType, out value))
			{
				return value(owner, req);
			}
			return true;
		}

		public bool ExecuteRequirements(object owner, List<Requirement> requirements, string removeReason)
		{
			var flag = true;
			if (requirements != null)
			{
				for (var i = 0; i < requirements.Count; i++)
				{
					var requirement = requirements[i];
					if (requirement.RequirementType == RequirementType.PayItem && owner is InventoryGameData)
					{
						flag &= DIContainerLogic.InventoryService.RemoveItem(owner as InventoryGameData, requirement.NameId, (int)requirement.Value, removeReason);
						if (flag && requirement.NameId == "lucky_coin")
						{
							DIContainerInfrastructure.GetProfileMgr().CurrentProfile.HardCurrencySpent += (int)requirement.Value;
						}
					}
				}
			}
			return flag;
		}

		public bool ExecuteRequirements(object owner, List<Requirement> requirements, Dictionary<string, string> trackingDictionary)
		{
			var flag = true;
			if (requirements != null)
			{
				for (var i = 0; i < requirements.Count; i++)
				{
					var requirement = requirements[i];
					if (requirement.RequirementType == RequirementType.PayItem && owner is InventoryGameData)
					{
						flag &= DIContainerLogic.InventoryService.RemoveItem(owner as InventoryGameData, requirement.NameId, (int)requirement.Value, trackingDictionary);
					}
				}
			}
			return flag;
		}

		public string ToString(Requirement req)
		{
			return req.RequirementType.ToString() + " " + req.NameId + " " + req.Value;
		}

		public string GetRequirementListString(List<Requirement> reqList)
		{
			var text = string.Empty;
			for (var i = 0; i < reqList.Count; i++)
			{
				var requirement = reqList[i];
				text += ToString(requirement);
				text += "\n";
			}
			return text;
		}

		public float GetRequirementValue(List<Requirement> reqList, RequirementType type, string nameId)
		{
			if (reqList == null)
			{
				return 0f;
			}
			for (var i = 0; i < reqList.Count; i++)
			{
				var requirement = reqList[i];
				if (requirement.RequirementType == type && requirement.NameId == nameId)
				{
					return requirement.Value;
				}
			}
			return 0f;
		}

		public static float GetWeaponEnchantmentAverage(List<ICombatant> birdList)
		{
			var num = 0f;
			for (var i = 0; i < birdList.Count; i++)
			{
				var combatant = birdList[i];
				num += (float)combatant.CombatantMainHandEquipment.EnchantmentLevel;
			}
			return num / (float)birdList.Count;
		}

		public static float GetWeaponEnchantmentHighAverage(List<BirdGameData> birdList)
		{
			var num = 0f;
			for (var i = 0; i < birdList.Count; i++)
			{
				var birdGameData = birdList[i];
				var num2 = 0;
				foreach (var item in birdGameData.InventoryGameData.Items[InventoryItemType.MainHandEquipment])
				{
					var equipmentGameData = item as EquipmentGameData;
					if (equipmentGameData != null && equipmentGameData.EnchantmentLevel > num2)
					{
						num2 = equipmentGameData.EnchantmentLevel;
					}
				}
				num += (float)num2;
			}
			return num / (float)birdList.Count;
		}
	}
}
