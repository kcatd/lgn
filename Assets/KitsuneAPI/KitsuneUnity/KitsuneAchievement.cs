using System;
using System.Collections.Generic;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using KitsuneCore.Services.Achievements;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Represents an achievement on the server that can give rewards
	/// A Unity ScriptableObject Wrapper for the Kitsune AchievementEntity
	/// </summary>
	/// <inheritdoc cref="AchievementEntity"/>
	[Serializable]
	public class KitsuneAchievement : UnityKitsuneEntity
	{		
		[SerializeField]
		private string _description;
		/// <inheritdoc cref="AchievementEntity.Description"/>
		public string Description => Entity.GetComponent<DescriptionComponent>().Value;

		/// <inheritdoc cref="AchievementEntity.PercentComplete"/>
		public double PercentComplete => GetComponent<ProgressComponent>().PercentComplete;
		
		/// <inheritdoc cref="AchievementEntity.Completed"/>
		public bool Completed => GetComponent<ProgressComponent>().Completed;
		
		[SerializeField] 
		private KitsuneReward _reward;
		/// <inheritdoc cref="AchievementEntity.RewardId"/>
		public EntityId RewardId => Entity.GetComponent<RewardComponent>().RewardId;
		
		[SerializeField] 
		private KitsuneAchievement _precondition;
		/// <inheritdoc cref="AchievementEntity.Precondition"/>
		public EntityId Precondition => Entity.GetComponent<RequiredAchievementsComponent>().Achievements[0];
		
		[SerializeField] 
		private string _objectiveType;
		/// <inheritdoc cref="AchievementEntity.ObjectiveType"/>
		public string ObjectiveType => Entity.GetComponent<ObjectiveComponent>().Type;
		
		[SerializeField] 
		private string _objectiveTarget;
		/// <inheritdoc cref="AchievementEntity.ObjectiveTarget"/>
		public string ObjectiveTarget => Entity.GetComponent<ObjectiveComponent>().Target;

		public override KitsuneEntity Entity
		{
			get
			{
				if (_entity == null)
				{
					_entity = new AchievementEntity();
					_entity.AddComponent(new DescriptionComponent());
					_entity.AddComponent(new ProgressComponent());
					_entity.AddComponent(new RewardComponent());
					_entity.AddComponent(new RequiredAchievementsComponent());
					_entity.AddComponent(new ObjectiveComponent());
					_entity.AddComponent(new VersionComponent());
				}
				
				_entity.Id = Id;
				_entity.VersionId = VersionId;
				_entity.GameId = KitsuneManager.GameSettings.GameId;
				_entity.ReleaseVersion = _releaseVersion;
				_entity.Name = Name;
				_entity.GetComponent<DescriptionComponent>().Value = _description;
				_entity.GetComponent<RewardComponent>().RewardId = _reward.Id;
				_entity.GetComponent<RequiredAchievementsComponent>().Achievements = new List<EntityId>{ _precondition ? _precondition.Id : EntityId.Zero };
				_entity.GetComponent<ObjectiveComponent>().Type = _objectiveType;
				_entity.GetComponent<ObjectiveComponent>().Target = _objectiveTarget;
				_entity.GetComponent<VersionComponent>().GameVersionId = GameVersionId.Mk(_releaseVersionId);

				return _entity;
			}
		}
	}
}