using UnityEngine;
using System;
using System.Collections.Generic;

namespace GuildMaster
{
    public enum JobClass
    {
        None,
        Warrior,    // 전사: 높은 HP와 물리 공격력
        Knight,     // 기사: 최고의 방어력과 아군 보호 능력
        Mage,       // 마법사: 강력한 마법 공격력과 광역 스킬
        Priest,     // 성직자: 힐링과 부활 마법의 전문가
        Assassin,   // 도적: 빠른 속도와 크리티컬 특화
        Ranger,     // 궁수: 원거리 물리 공격의 달인
        Sage        // 현자: 마법과 물리를 아우르는 만능형
    }
    
    public enum UnitRank
    {
        Common,     // 일반
        Uncommon,   // 희귀
        Rare,       // 희귀
        Epic,       // 영웅
        Legendary   // 전설
    }

    [System.Serializable]
    public class Unit
    {
        // Basic Info
        public string UnitId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public JobClass JobClass { get; set; }
        public UnitRank Rank { get; set; }
        public bool IsPlayerUnit { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        
        // Job Mastery
        public float JobMastery { get; set; } // 0-100
        public int AwakeningLevel { get; set; } // 0-5 각성 레벨

        // Position in Battle
        public int SquadIndex { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        // Base Stats
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxMana { get; set; }
        public float CurrentMana { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }
        public float MagicPower { get; set; }
        public float Speed { get; set; }
        public float CriticalRate { get; set; }
        public float CriticalDamage { get; set; }
        public float Accuracy { get; set; }
        public float Evasion { get; set; }
        
        // Shield system
        public float CurrentShield { get; set; }
        public float MaxShield { get; set; }

        // State
        public bool IsAlive => CurrentHealth > 0;
        
        // Combat Stats (Calculated)
        public float AttackPower => CalculateAttackPower();
        public float MagicAttackPower => CalculateMagicAttackPower();
        public float PhysicalDefense => CalculatePhysicalDefense();
        public float MagicalDefense => CalculateMagicalDefense();
        public float HealingPower => CalculateHealingPower();
        
        // Job-specific abilities
        public class JobAbility
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public float Chance { get; set; } // Chance to trigger
            public float Value { get; set; }   // Effect value
        }
        
        private List<JobAbility> jobAbilities = new List<JobAbility>();

        // Events
        public event Action<Unit, float> OnDamageTaken;
        public event Action<Unit, float> OnHealed;
        public event Action<Unit> OnDeath;

        public Unit(string name, int level, JobClass jobClass, UnitRank rank = UnitRank.Common)
        {
            UnitId = Guid.NewGuid().ToString();
            Name = name;
            Level = level;
            JobClass = jobClass;
            Rank = rank;
            Experience = 0;
            ExperienceToNextLevel = CalculateExpToNextLevel();
            JobMastery = 0f;
            AwakeningLevel = 0;
            
            InitializeStats();
            InitializeJobAbilities();
        }
        
        int CalculateExpToNextLevel()
        {
            return 100 * Level * (int)Mathf.Pow(1.2f, Level - 1);
        }

        void InitializeStats()
        {
            // Apply rank multiplier
            float rankMultiplier = GetRankMultiplier();
            
            // Base stats by job class
            switch (JobClass)
            {
                case JobClass.Warrior:
                    MaxHealth = (100 + (Level * 20)) * rankMultiplier;
                    MaxMana = (50 + (Level * 5)) * rankMultiplier;
                    Attack = (15 + (Level * 3)) * rankMultiplier;
                    Defense = (10 + (Level * 2)) * rankMultiplier;
                    MagicPower = (5 + (Level * 0.5f)) * rankMultiplier;
                    Speed = (8 + (Level * 1)) * rankMultiplier;
                    CriticalRate = 0.15f + (Rank == UnitRank.Legendary ? 0.1f : 0f);
                    CriticalDamage = 1.5f + (AwakeningLevel * 0.1f);
                    Accuracy = 0.9f;
                    Evasion = 0.05f;
                    break;
                    
                case JobClass.Knight:
                    MaxHealth = (120 + (Level * 25)) * rankMultiplier;
                    MaxMana = (60 + (Level * 6)) * rankMultiplier;
                    Attack = (12 + (Level * 2)) * rankMultiplier;
                    Defense = (15 + (Level * 3)) * rankMultiplier;
                    MagicPower = (8 + (Level * 1)) * rankMultiplier;
                    Speed = (6 + (Level * 0.8f)) * rankMultiplier;
                    CriticalRate = 0.1f;
                    CriticalDamage = 1.4f + (AwakeningLevel * 0.08f);
                    Accuracy = 0.85f;
                    Evasion = 0.03f + (JobMastery * 0.001f);
                    break;
                    
                case JobClass.Mage:
                    MaxHealth = (60 + (Level * 10)) * rankMultiplier;
                    MaxMana = (100 + (Level * 15)) * rankMultiplier;
                    Attack = (5 + (Level * 0.5f)) * rankMultiplier;
                    Defense = (5 + (Level * 1)) * rankMultiplier;
                    MagicPower = (20 + (Level * 4)) * rankMultiplier;
                    Speed = (10 + (Level * 1.2f)) * rankMultiplier;
                    CriticalRate = 0.2f + (JobMastery * 0.002f);
                    CriticalDamage = 1.8f + (AwakeningLevel * 0.12f);
                    Accuracy = 0.95f;
                    Evasion = 0.08f;
                    break;
                    
                case JobClass.Priest:
                    MaxHealth = (70 + (Level * 12)) * rankMultiplier;
                    MaxMana = (80 + (Level * 12)) * rankMultiplier;
                    Attack = (8 + (Level * 1)) * rankMultiplier;
                    Defense = (8 + (Level * 1.5f)) * rankMultiplier;
                    MagicPower = (15 + (Level * 3)) * rankMultiplier;
                    Speed = (9 + (Level * 1)) * rankMultiplier;
                    CriticalRate = 0.05f;
                    CriticalDamage = 1.3f;
                    Accuracy = 0.9f;
                    Evasion = 0.06f;
                    break;
                    
                case JobClass.Assassin:
                    MaxHealth = (80 + (Level * 15)) * rankMultiplier;
                    MaxMana = (60 + (Level * 8)) * rankMultiplier;
                    Attack = (18 + (Level * 3.5f)) * rankMultiplier;
                    Defense = (7 + (Level * 1.2f)) * rankMultiplier;
                    MagicPower = (5 + (Level * 0.5f)) * rankMultiplier;
                    Speed = (15 + (Level * 2)) * rankMultiplier;
                    CriticalRate = 0.35f + (JobMastery * 0.003f);
                    CriticalDamage = 2.0f + (AwakeningLevel * 0.15f);
                    Accuracy = 0.95f;
                    Evasion = 0.15f + (JobMastery * 0.002f);
                    break;
                    
                case JobClass.Ranger:
                    MaxHealth = (85 + (Level * 16)) * rankMultiplier;
                    MaxMana = (70 + (Level * 9)) * rankMultiplier;
                    Attack = (16 + (Level * 3.2f)) * rankMultiplier;
                    Defense = (8 + (Level * 1.5f)) * rankMultiplier;
                    MagicPower = (5 + (Level * 0.5f)) * rankMultiplier;
                    Speed = (12 + (Level * 1.5f)) * rankMultiplier;
                    CriticalRate = 0.25f + (JobMastery * 0.0025f);
                    CriticalDamage = 1.7f + (AwakeningLevel * 0.1f);
                    Accuracy = 0.98f;
                    Evasion = 0.1f;
                    break;
                    
                case JobClass.Sage:
                    MaxHealth = (90 + (Level * 18)) * rankMultiplier;
                    MaxMana = (120 + (Level * 18)) * rankMultiplier;
                    Attack = (12 + (Level * 2)) * rankMultiplier;
                    Defense = (10 + (Level * 2)) * rankMultiplier;
                    MagicPower = (18 + (Level * 3.5f)) * rankMultiplier;
                    Speed = (11 + (Level * 1.3f)) * rankMultiplier;
                    CriticalRate = 0.15f + (JobMastery * 0.0015f);
                    CriticalDamage = 1.6f + (AwakeningLevel * 0.1f);
                    Accuracy = 0.92f;
                    Evasion = 0.07f;
                    break;
            }
            
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            CurrentShield = 0;
            MaxShield = MaxHealth * 0.3f;
        }
        
        float GetRankMultiplier()
        {
            return Rank switch
            {
                UnitRank.Common => 1f,
                UnitRank.Uncommon => 1.15f,
                UnitRank.Rare => 1.3f,
                UnitRank.Epic => 1.5f,
                UnitRank.Legendary => 1.8f,
                _ => 1f
            };
        }

        public void SetPosition(int squadIndex, int row, int col)
        {
            SquadIndex = squadIndex;
            Row = row;
            Col = col;
        }

        public float GetAttackDamage()
        {
            float baseDamage;
            
            // Magic classes use magic power
            if (JobClass == JobClass.Mage || JobClass == JobClass.Priest || JobClass == JobClass.Sage)
            {
                baseDamage = MagicPower;
            }
            else
            {
                baseDamage = Attack;
            }
            
            // Apply critical hit
            bool isCritical = UnityEngine.Random.value < CriticalRate;
            if (isCritical)
            {
                baseDamage *= CriticalDamage;
            }
            
            // Apply accuracy
            if (UnityEngine.Random.value > Accuracy)
            {
                return 0; // Miss
            }
            
            // Add some randomness (90% - 110%)
            baseDamage *= UnityEngine.Random.Range(0.9f, 1.1f);
            
            return baseDamage;
        }

        public float GetHealPower()
        {
            float healPower = MagicPower * 0.8f;
            
            // Priests get healing bonus
            if (JobClass == JobClass.Priest)
            {
                healPower *= 1.5f;
            }
            // Sages get smaller healing bonus
            else if (JobClass == JobClass.Sage)
            {
                healPower *= 1.2f;
            }
            
            // Add some randomness
            healPower *= UnityEngine.Random.Range(0.9f, 1.1f);
            
            return healPower;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            
            // Check evasion
            if (UnityEngine.Random.value < Evasion)
            {
                OnDamageTaken?.Invoke(this, 0); // Evaded
                return;
            }
            
            // Apply defense
            float actualDamage = damage - (Defense * 0.5f);
            actualDamage = Mathf.Max(1, actualDamage); // Minimum 1 damage
            
            // Apply damage to shield first
            if (CurrentShield > 0)
            {
                float shieldDamage = Mathf.Min(CurrentShield, actualDamage);
                CurrentShield -= shieldDamage;
                actualDamage -= shieldDamage;
            }
            
            // Apply remaining damage to health
            CurrentHealth -= actualDamage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);
            
            OnDamageTaken?.Invoke(this, actualDamage);
            
            if (!IsAlive)
            {
                OnDeath?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            
            float actualHeal = Mathf.Min(amount, MaxHealth - CurrentHealth);
            CurrentHealth += actualHeal;
            
            OnHealed?.Invoke(this, actualHeal);
        }

        public void Revive(float healthPercentage = 0.5f)
        {
            if (IsAlive) return;
            
            CurrentHealth = MaxHealth * healthPercentage;
        }

        public float GetHealthPercentage()
        {
            return CurrentHealth / MaxHealth;
        }

        public string GetJobIcon()
        {
            switch (JobClass)
            {
                case JobClass.Warrior: return "⚔️";
                case JobClass.Knight: return "🛡️";
                case JobClass.Mage: return "🧙";
                case JobClass.Priest: return "✨";
                case JobClass.Assassin: return "🗡️";
                case JobClass.Ranger: return "🏹";
                case JobClass.Sage: return "📖";
                default: return "❓";
            }
        }
        
        public void AddShield(float amount)
        {
            CurrentShield = Mathf.Min(CurrentShield + amount, MaxShield);
        }
        
        public void RestoreMana(float amount)
        {
            CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
        }
        
        public float GetManaPercentage()
        {
            return MaxMana > 0 ? CurrentMana / MaxMana : 0;
        }
        
        // Job Abilities System
        void InitializeJobAbilities()
        {
            jobAbilities.Clear();
            
            switch (JobClass)
            {
                case JobClass.Warrior:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "분노의 일격",
                        Description = "체력이 50% 이하일 때 공격력 증가",
                        Chance = 1f,
                        Value = 0.3f + (JobMastery * 0.003f)
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "반격",
                        Description = "피격 시 반격 확률",
                        Chance = 0.15f + (JobMastery * 0.001f),
                        Value = 0.5f
                    });
                    break;
                    
                case JobClass.Knight:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "수호자의 맹세",
                        Description = "아군 보호 시 피해 감소",
                        Chance = 0.3f + (JobMastery * 0.002f),
                        Value = 0.5f
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "천상의 축복",
                        Description = "매 턴 체력 회복",
                        Chance = 0.2f,
                        Value = MaxHealth * 0.02f
                    });
                    break;
                    
                case JobClass.Mage:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "원소 폭발",
                        Description = "마법 공격 시 추가 피해",
                        Chance = 0.25f + (JobMastery * 0.002f),
                        Value = 0.4f
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "마나 흡수",
                        Description = "킬 시 마나 회복",
                        Chance = 0.3f,
                        Value = 0.1f
                    });
                    break;
                    
                case JobClass.Priest:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "성스러운 광휘",
                        Description = "힐링 효과 증폭",
                        Chance = 1f,
                        Value = 0.5f + (JobMastery * 0.005f)
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "기적",
                        Description = "죽은 아군 부활 확률",
                        Chance = 0.05f + (AwakeningLevel * 0.01f),
                        Value = 0.3f
                    });
                    break;
                    
                case JobClass.Assassin:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "그림자 걸음",
                        Description = "회피률 대폭 증가",
                        Chance = 1f,
                        Value = 0.5f
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "치명타",
                        Description = "크리티컬 시 즉사 확률",
                        Chance = 0.01f + (JobMastery * 0.0005f),
                        Value = 1f
                    });
                    break;
                    
                case JobClass.Ranger:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "매의 눈",
                        Description = "명중률 100%",
                        Chance = 1f,
                        Value = 1f
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "다중 사격",
                        Description = "추가 대상 공격",
                        Chance = 0.2f + (JobMastery * 0.001f),
                        Value = 2f
                    });
                    break;
                    
                case JobClass.Sage:
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "지혜의 광휘",
                        Description = "모든 능력치 상승",
                        Chance = 1f,
                        Value = 0.1f + (JobMastery * 0.001f)
                    });
                    jobAbilities.Add(new JobAbility
                    {
                        Name = "전능",
                        Description = "물리/마법 복합 공격",
                        Chance = 0.3f,
                        Value = 1f
                    });
                    break;
            }
        }
        
        // Combat calculations with job abilities
        float CalculateAttackPower()
        {
            float basePower = Attack;
            
            // Warrior rage bonus
            if (JobClass == JobClass.Warrior && GetHealthPercentage() < 0.5f)
            {
                var rageAbility = jobAbilities.Find(a => a.Name == "분노의 일격");
                if (rageAbility != null)
                    basePower *= (1 + rageAbility.Value);
            }
            
            // Sage wisdom bonus
            if (JobClass == JobClass.Sage)
            {
                var wisdomAbility = jobAbilities.Find(a => a.Name == "지혜의 광휘");
                if (wisdomAbility != null)
                    basePower *= (1 + wisdomAbility.Value);
            }
            
            return basePower;
        }
        
        float CalculateMagicAttackPower()
        {
            float basePower = MagicPower;
            
            // Mage elemental explosion
            if (JobClass == JobClass.Mage)
            {
                var explosionAbility = jobAbilities.Find(a => a.Name == "원소 폭발");
                if (explosionAbility != null && UnityEngine.Random.value < explosionAbility.Chance)
                    basePower *= (1 + explosionAbility.Value);
            }
            
            // Sage wisdom bonus
            if (JobClass == JobClass.Sage)
            {
                var wisdomAbility = jobAbilities.Find(a => a.Name == "지혜의 광휘");
                if (wisdomAbility != null)
                    basePower *= (1 + wisdomAbility.Value);
            }
            
            return basePower;
        }
        
        float CalculatePhysicalDefense()
        {
            float baseDefense = Defense;
            
            // Knight guardian's oath
            if (JobClass == JobClass.Knight)
            {
                baseDefense *= 1.2f; // Passive 20% defense bonus
            }
            
            return baseDefense;
        }
        
        float CalculateMagicalDefense()
        {
            float baseDefense = Defense * 0.7f + MagicPower * 0.3f;
            
            // Priest holy protection
            if (JobClass == JobClass.Priest)
            {
                baseDefense *= 1.15f;
            }
            
            return baseDefense;
        }
        
        float CalculateHealingPower()
        {
            float baseHeal = MagicPower * 0.8f;
            
            // Priest holy radiance
            if (JobClass == JobClass.Priest)
            {
                var radianceAbility = jobAbilities.Find(a => a.Name == "성스러운 광휘");
                if (radianceAbility != null)
                    baseHeal *= (1 + radianceAbility.Value);
            }
            // Sage gets smaller healing bonus
            else if (JobClass == JobClass.Sage)
            {
                baseHeal *= 1.2f;
            }
            
            return baseHeal;
        }
        
        // Level up and awakening
        public bool CanLevelUp()
        {
            return Experience >= ExperienceToNextLevel;
        }
        
        public void LevelUp()
        {
            if (!CanLevelUp()) return;
            
            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel = CalculateExpToNextLevel();
            
            // Reinitialize stats with new level
            InitializeStats();
            
            // Increase job mastery
            JobMastery = Mathf.Min(100f, JobMastery + 2f);
            
            // Update abilities
            InitializeJobAbilities();
        }
        
        public bool CanAwaken()
        {
            return Level >= 50 && AwakeningLevel < 5;
        }
        
        public void Awaken()
        {
            if (!CanAwaken()) return;
            
            AwakeningLevel++;
            
            // Boost all stats
            MaxHealth *= 1.1f;
            MaxMana *= 1.1f;
            Attack *= 1.1f;
            Defense *= 1.1f;
            MagicPower *= 1.1f;
            Speed *= 1.1f;
            
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            
            // Update abilities
            InitializeJobAbilities();
        }
        
        public List<JobAbility> GetJobAbilities()
        {
            return new List<JobAbility>(jobAbilities);
        }
        
        public string GetRankColor()
        {
            return Rank switch
            {
                UnitRank.Common => "#FFFFFF",
                UnitRank.Uncommon => "#00FF00",
                UnitRank.Rare => "#0080FF",
                UnitRank.Epic => "#B400FF",
                UnitRank.Legendary => "#FF8000",
                _ => "#FFFFFF"
            };
        }
    }
}