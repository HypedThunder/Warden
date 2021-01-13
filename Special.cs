using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.Treebot.Weapon;
using EntityStates.HAND.Weapon;
using EntityStates.NullifierMonster;
using EntityStates.GravekeeperBoss;

namespace EntityStates.Ranger.Weapon4
{
	// Token: 0x020008F2 RID: 2290
	public class Voidcapture : BaseState
	{
		// Token: 0x0600360A RID: 13834 RVA: 0x000DC0A6 File Offset: 0x000DA2A6
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = FireSyringe.baseDuration / this.attackSpeedStat;
			this.fireDuration = FireSyringe.baseFireDuration / this.attackSpeedStat;
		}

		// Token: 0x0600360B RID: 13835 RVA: 0x00032FA7 File Offset: 0x000311A7
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x0600360C RID: 13836 RVA: 0x000DC0D4 File Offset: 0x000DA2D4
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			int num = Mathf.FloorToInt(base.fixedAge / this.fireDuration * (float)FireSyringe.projectileCount);
			if (this.projectilesFired <= num && this.projectilesFired < FireSyringe.projectileCount)
			{
				GameObject prefab = FireHook.projectilePrefab;
				string soundString = FirePortalBomb.muzzleString;
				if (this.projectilesFired == FireSyringe.projectileCount - 1)
				{
					prefab = FireHook.projectilePrefab;
					soundString = NullifierMonster.SpawnState.spawnSoundString;
				}
				base.PlayAnimation("Gesture", "FireFMJ", "FireFMJ.playbackRate", this.duration);
				Util.PlaySound(soundString, base.gameObject);
				base.characterBody.SetAimTimer(3f);
				if (DeathState.deathExplosionEffect)
				{
					EffectManager.SimpleMuzzleFlash(DeathState.deathExplosionEffect, base.gameObject, FireSyringe.muzzleName, false);
				}
				if (base.isAuthority)
				{
					Ray aimRay = base.GetAimRay();
					float bonusYaw = (float)Mathf.FloorToInt((float)this.projectilesFired - (float)(FireSyringe.projectileCount - 1) / 2f) / (float)(FireSyringe.projectileCount - 1) * FireSyringe.totalYawSpread;
					Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, bonusYaw, 0f);
					ProjectileManager.instance.FireProjectile(prefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, this.damageStat * 5f, FireSyringe.force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
				}
				this.projectilesFired++;
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x0600360D RID: 13837 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		// Token: 0x04002E7B RID: 11899
		public static GameObject projectilePrefab;

		// Token: 0x04002E7C RID: 11900
		public static GameObject finalProjectilePrefab;

		// Token: 0x04002E7D RID: 11901
		public static GameObject muzzleflashEffectPrefab;

		// Token: 0x04002E7E RID: 11902
		public static int projectileCount = 3;

		// Token: 0x04002E7F RID: 11903
		public static float totalYawSpread = 5f;

		// Token: 0x04002E80 RID: 11904
		public static float baseDuration = 2f;

		// Token: 0x04002E81 RID: 11905
		public static float baseFireDuration = 0.2f;

		// Token: 0x04002E82 RID: 11906
		public static float damageCoefficient = 1.2f;

		// Token: 0x04002E83 RID: 11907
		public static float force = 20f;

		// Token: 0x04002E84 RID: 11908
		public static string attackSound;

		// Token: 0x04002E85 RID: 11909
		public static string finalAttackSound;

		// Token: 0x04002E86 RID: 11910
		public static string muzzleName;

		// Token: 0x04002E87 RID: 11911
		private float duration;

		// Token: 0x04002E88 RID: 11912
		private float fireDuration;

		// Token: 0x04002E89 RID: 11913
		private int projectilesFired;
	}
}