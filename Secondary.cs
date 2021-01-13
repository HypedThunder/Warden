using System;
using EntityStates.ClayBoss;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.NullifierMonster;
using RoR2;
using UnityEngine;

namespace EntityStates.Ranger.Weapon2
{
	// Token: 0x02000007 RID: 7
	public class Voidgun : BaseSkillState
	{
		// Token: 0x06000020 RID: 32 RVA: 0x000032EC File Offset: 0x000014EC
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = Voidgun.baseDuration / this.attackSpeedStat;
			this.fireDuration = 0.2f * this.duration;
			base.characterBody.SetAimTimer(2f);
			this.animator = base.GetModelAnimator();
			this.muzzleName = "MuzzleShotgun";
			base.PlayAnimation("Gesture, Additive", "FireShotgun", "FireShotgun.playbackRate", this.duration * 1.1f);
			base.PlayAnimation("Gesture, Override", "FireShotgun", "FireShotgun.playbackRate", this.duration * 1.1f);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003391 File Offset: 0x00001591
		public override void OnExit()
		{
			base.OnExit();
			base.PlayAnimation("Gesture, Override", "BufferEmpty");
			base.PlayAnimation("Gesture, Additive", "BufferEmpty");
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000033C0 File Offset: 0x000015C0
		private void ThrowTar()
		{
			bool flag = !this.hasFired;
			if (flag)
			{
				this.hasFired = true;
				Util.PlaySound(EntityStates.NullifierMonster.SpawnState.spawnSoundString, base.gameObject);
				EffectManager.SimpleMuzzleFlash(FirePortalBomb.muzzleflashEffectPrefab, base.gameObject, this.muzzleName, false);
				bool isAuthority = base.isAuthority;
				if (isAuthority)
				{
					float damage = 2f * this.damageStat;
					Ray aimRay = base.GetAimRay();
					new BulletAttack
					{
						bulletCount = 3,
						aimVector = aimRay.direction,
						origin = aimRay.origin,
						damage = damage,
						damageColorIndex = DamageColorIndex.Default,
						damageType = DamageType.Nullify,
						falloffModel = BulletAttack.FalloffModel.None,
						maxDistance = 48f,
						force = 10f,
						hitMask = LayerIndex.CommonMasks.bullet,
						minSpread = 3f,
						maxSpread = 3f,
						isCrit = base.RollCrit(),
						owner = base.gameObject,
						muzzleName = "MuzzleShotgun",
						smartCollision = false,
						procChainMask = default(ProcChainMask),
						procCoefficient = 1f,
						radius = 0.75f,
						sniper = false,
						stopperMask = LayerIndex.CommonMasks.bullet,
						weapon = null,
						tracerEffectPrefab = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletTracerEffectPrefab,
						spreadPitchScale = 0.5f,
						spreadYawScale = 0.5f,
						queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
						hitEffectPrefab = FirePortalBomb.portalBombProjectileEffect,
						HitEffectNormal = FirePortalBomb.portalBombProjectileEffect
					}.Fire();
				}
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003568 File Offset: 0x00001768
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			bool flag = base.fixedAge >= this.fireDuration;
			if (flag)
			{
				this.ThrowTar();
			}
			bool flag2 = base.fixedAge >= this.duration && base.isAuthority;
			if (flag2)
			{
				this.outer.SetNextStateToMain();
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000035C4 File Offset: 0x000017C4
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		// Token: 0x04000029 RID: 41
		public static float damageCoefficient = 1.5f;

		// Token: 0x0400002A RID: 42
		public static float baseDuration = 0.6f;

		// Token: 0x0400002B RID: 43
		public static float bulletForce = 25f;

		// Token: 0x0400002C RID: 44
		public static float projectileCount = 8f;

		// Token: 0x0400002D RID: 45
		public static GameObject bulletTracer = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerBanditPistol");

		// Token: 0x0400002E RID: 46
		private float duration;

		// Token: 0x0400002F RID: 47
		private float fireDuration;

		// Token: 0x04000030 RID: 48
		private bool hasFired;

		// Token: 0x04000031 RID: 49
		private Animator animator;

		// Token: 0x04000032 RID: 50
		private string muzzleName;
	}
}
