using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Ranger.Weapon;
using EntityStates.Ranger.Weapon2;
using EntityStates.Ranger.Weapon3;
using EntityStates.Ranger.Weapon4;
using RoR2.Projectile;

namespace Ranger
{
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "Warden", "0.0.2")]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(BuffAPI), nameof(EffectAPI))]

    public class WardenMod : BaseUnityPlugin
    {
        public const string MODUID = "com.Ruxbieno.Warden";

        internal static WardenMod instance;

        public static GameObject myCharacter;
        public static GameObject characterDisplay;
        public static GameObject doppelganger;

        public static GameObject wardenCrosshair;

        private static readonly Color CHAR_COLOR = new Color(0.2f, 0f, 0.2f);

        private static ConfigEntry<float> baseHealth;
        private static ConfigEntry<float> healthGrowth;
        private static ConfigEntry<float> baseArmor;
        private static ConfigEntry<float> baseDamage;
        private static ConfigEntry<float> damageGrowth;
        private static ConfigEntry<float> baseRegen;
        private static ConfigEntry<float> regenGrowth;
        private static ConfigEntry<float> baseSpeed;

        public void Awake()
        {
            instance = this;

            ReadConfig();
            RegisterStates();
            RegisterCharacter();
            Skins.RegisterSkins();
            CreateMaster();
        }

        private void ReadConfig()
        {
            baseHealth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health"), 115f, new ConfigDescription("Base health", null, Array.Empty<object>()));
            healthGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health growth"), 30f, new ConfigDescription("Health per level", null, Array.Empty<object>()));
            baseArmor = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Armor"), 10f, new ConfigDescription("Base armor", null, Array.Empty<object>()));
            baseDamage = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage"), 12f, new ConfigDescription("Base damage", null, Array.Empty<object>()));
            damageGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage growth"), 2.4f, new ConfigDescription("Damage per level", null, Array.Empty<object>()));
            baseRegen = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen"), 1f, new ConfigDescription("Base HP regen", null, Array.Empty<object>()));
            regenGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen growth"), 0.5f, new ConfigDescription("HP regen per level", null, Array.Empty<object>()));
            baseSpeed = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Speed"), 7f, new ConfigDescription("Base speed", null, Array.Empty<object>()));
        }

           private void RegisterCharacter()
        {
            //create a clone of the grovetender prefab
            myCharacter = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody"), "Prefabs/CharacterBodies/WardenBody", true);
            //create a display prefab
            characterDisplay = PrefabAPI.InstantiateClone(myCharacter.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "WardenDisplay", true);


            //add custom menu animation script
            characterDisplay.AddComponent<MenuAnim>();


            CharacterBody charBody = myCharacter.GetComponent<CharacterBody>();
            charBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;

            //swap to generic mainstate to fix clunky controls
            myCharacter.GetComponent<EntityStateMachine>().mainStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));

            myCharacter.GetComponentInChildren<Interactor>().maxInteractionDistance = 5f;

            //crosshair stuff
            charBody.SetSpreadBloom(0, false);
            charBody.spreadBloomCurve = Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody").GetComponent<CharacterBody>().spreadBloomCurve;
            charBody.spreadBloomDecayTime = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CharacterBody>().spreadBloomDecayTime;

            charBody.hullClassification = HullClassification.Human;


            characterDisplay.AddComponent<NetworkIdentity>();

            //create the custom crosshair
            wardenCrosshair = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshairSmall"), "WardenCrosshair", true);
            wardenCrosshair.AddComponent<NetworkIdentity>();

            //networking

            if (myCharacter) PrefabAPI.RegisterNetworkPrefab(myCharacter);
            if (characterDisplay) PrefabAPI.RegisterNetworkPrefab(characterDisplay);
            if (doppelganger) PrefabAPI.RegisterNetworkPrefab(doppelganger);
            if (wardenCrosshair) PrefabAPI.RegisterNetworkPrefab(wardenCrosshair);



            string desc = "The Warden is a debilitating survivor that makes use of containment measures to trap his enemies and kill them.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Six Shooter fires much faster if you rapidly press it." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Capture is the easiest way to make good use of your Passive." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Void Warp is good for upkeeeping a steady source of damage." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Entrapment deals heavy damage and makes it easier for you to deal more damage to enemies.</color>" + Environment.NewLine;

            LanguageAPI.Add("WARDEN_NAME", "Warden");
            LanguageAPI.Add("WARDEN_DESCRIPTION", desc);
            LanguageAPI.Add("WARDEN_SUBTITLE", "Call of the Void");
            LanguageAPI.Add("WARDEN_OUTRO_FLAVOR", "...and so he left, an endless wanderer.");


            charBody.name = "WardenBody";
            charBody.baseNameToken = "WARDEN_NAME";
            charBody.subtitleNameToken = "WARDEN_SUBTITLE";
            charBody.crosshairPrefab = wardenCrosshair;

            charBody.baseMaxHealth = baseHealth.Value;
            charBody.levelMaxHealth = healthGrowth.Value;
            charBody.baseRegen = baseRegen.Value;
            charBody.levelRegen = regenGrowth.Value;
            charBody.baseDamage = baseDamage.Value;
            charBody.levelDamage = damageGrowth.Value;
            charBody.baseArmor = baseArmor.Value;
            charBody.baseMoveSpeed = baseSpeed.Value;
            charBody.levelArmor = 0;
            charBody.baseCrit = 1;

            charBody.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/NullifierBody").GetComponent<CharacterBody>().preferredPodPrefab;


            //create a survivordef for our grovetender
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "WARDEN_NAME",
                unlockableName = "",
                descriptionToken = "WARDEN_DESCRIPTION",
                primaryColor = CHAR_COLOR,
                bodyPrefab = myCharacter,
                displayPrefab = characterDisplay,
                outroFlavorToken = "WARDEN_OUTRO_FLAVOR"
            };


            SurvivorAPI.AddSurvivor(survivorDef);


            SkillSetup();


            //add it to the body catalog
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(myCharacter);
            };
        }

        private void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(Revolver));
            LoadoutAPI.AddSkill(typeof(Voidgun));
            LoadoutAPI.AddSkill(typeof(WarpDash));
            LoadoutAPI.AddSkill(typeof(Voidcapture));
        }

        private void SkillSetup()
        {
            foreach (GenericSkill obj in myCharacter.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        private void PrimarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Fire a heavy piercing bullet, dealing <style=cIsDamage>300% damage</style>.";

            LanguageAPI.Add("WARDEN_PRIMARY_REVOLVER_NAME", "Six Shooter");
            LanguageAPI.Add("WARDEN_PRIMARY_REVOLVER_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Revolver));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 6;
            mySkillDef.baseRechargeInterval = 1.4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 2;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "WARDEN_PRIMARY_REVOLVER_DESCRIPTION";
            mySkillDef.skillName = "WARDEN_PRIMARY_REVOLVER_NAME";
            mySkillDef.skillNameToken = "WARDEN_PRIMARY_REVOLVER_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);
            component.primary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue<SkillFamily>(component.primary, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.primary.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }


        private void SecondarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Release a blast of projectiles from the void, dealing <style=cIsDamage>3x200% damage</style> and <style=cIsUtility>entrapping enemies</style>.";

            LanguageAPI.Add("WARDEN_SECONDARY_SHOTGUN_NAME", "Capture");
            LanguageAPI.Add("WARDEN_SECONDARY_SHOTGUN_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Voidgun));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "WARDEN_SECONDARY_SHOTGUN_DESCRIPTION";
            mySkillDef.skillName = "WARDEN_SECONDARY_SHOTGUN_NAME";
            mySkillDef.skillNameToken = "WARDEN_SECONDARY_SHOTGUN_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.secondary.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }


        private void UtilitySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Warp in the direction you're facing, <style=cIsDamage>reloading your Primary fully</style>.";

            LanguageAPI.Add("WARDEN_UTILITY_RELOAD_NAME", "Void Warp");
            LanguageAPI.Add("WARDEN_UTILITY_RELOAD_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(WarpDash));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseRechargeInterval = 5;
            mySkillDef.baseMaxStock = 1;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "WARDEN_UTILITY_RELOAD_DESCRIPTION";
            mySkillDef.skillName = "WARDEN_UTILITY_RELOAD_NAME";
            mySkillDef.skillNameToken = "WARDEN_UTILITY_RELOAD_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void SpecialSetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Release <style=cIsUtility>homing</style> chains from the void, dealing <style=cIsDamage>4x400% damage</style>.";

            LanguageAPI.Add("WARDEN_SPECIAL_ENTRAPMENT_NAME", "Entrapment");
            LanguageAPI.Add("WARDEN_SPECIAL_ENTRAPMENT_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Voidcapture));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 7;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "WARDEN_SPECIAL_ENTRAPMENT_DESCRIPTION";
            mySkillDef.skillName = "WARDEN_SPECIAL_ENTRAPMENT_NAME";
            mySkillDef.skillNameToken = "WARDEN_SPECIAL_ENTRAPMENT_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.special.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void CreateMaster()
        {
            //create the doppelganger, uses commando ai bc i can't be bothered writing my own
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/MageMonsterMaster"), "RangerMonsterMaster", true);

            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = myCharacter;
        }


        public class MenuAnim : MonoBehaviour
        {
            //animates him in character select
            internal void OnEnable()
            {
                bool flag = base.gameObject.transform.parent.gameObject.name == "CharacterPad";
                if (flag)
                {
                    base.StartCoroutine(this.SpawnAnim());
                }
            }

            private IEnumerator SpawnAnim()
            {
                Animator animator = base.GetComponentInChildren<Animator>();
                Transform effectTransform = base.gameObject.transform;

                ChildLocator component = base.gameObject.GetComponentInChildren<ChildLocator>();

                if (component) effectTransform = component.FindChild("Root");

                GameObject.Instantiate<GameObject>(EntityStates.NullifierMonster.SpawnState.spawnEffectPrefab, effectTransform.position, Quaternion.identity);


                PlayAnimation("Body", "Spawn", "Spawn.playbackRate", 3, animator);

                yield break;
            }


            private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
            {
                int layerIndex = animator.GetLayerIndex(layerName);
                animator.SetFloat(playbackRateParam, 1f);
                animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
                animator.Update(0f);
                float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
                animator.SetFloat(playbackRateParam, length / duration);
            }
        }
    }


}
