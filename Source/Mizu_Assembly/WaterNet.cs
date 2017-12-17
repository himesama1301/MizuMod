using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class WaterNet
    {
        private static int nextID = 1;
        public static void ClearNextID()
        {
            nextID = 1;
        }

        public int ID = 0;

        private List<IBuilding_WaterNet> allThings = new List<IBuilding_WaterNet>();
        public List<IBuilding_WaterNet> AllThings
        {
            get
            {
                return allThings;
            }
        }

        private Dictionary<CompProperties_WaterNetInput.InputType, HashSet<IBuilding_WaterNet>> inputterTypeDic = new Dictionary<CompProperties_WaterNetInput.InputType, HashSet<IBuilding_WaterNet>>();
        private HashSet<IBuilding_WaterNet> outputters = new HashSet<IBuilding_WaterNet>();
        private HashSet<IBuilding_WaterNet> tanks = new HashSet<IBuilding_WaterNet>();

        private WaterType waterType = WaterType.NoWater;
        public WaterType WaterType
        {
            get
            {
                return this.waterType;
            }
        }
        private WaterType storedWaterType = WaterType.NoWater;
        public WaterType StoredWaterType
        {
            get
            {
                return this.storedWaterType;
            }
        }

        public MapComponent_WaterNetManager Manager { get; set; }

        public WaterNet()
        {
            this.ID = nextID;
            nextID++;

            this.waterType = WaterType.NoWater;
            this.storedWaterType = WaterType.NoWater;

            this.inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterNet] = new HashSet<IBuilding_WaterNet>();
            this.inputterTypeDic[CompProperties_WaterNetInput.InputType.Rain] = new HashSet<IBuilding_WaterNet>();
            this.inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterPool] = new HashSet<IBuilding_WaterNet>();
            this.inputterTypeDic[CompProperties_WaterNetInput.InputType.Terrain] = new HashSet<IBuilding_WaterNet>();
        }

        public WaterNet(IBuilding_WaterNet thing) : this()
        {
            this.AddThing(thing);
        }

        public void AddThing(IBuilding_WaterNet thing)
        {
            thing.InputWaterNet = this;
            thing.OutputWaterNet = this;

            this.AddThingToList(thing);
        }
        public void AddInputThing(IBuilding_WaterNet thing)
        {
            thing.InputWaterNet = this;

            this.AddThingToList(thing);
        }
        public void AddOutputThing(IBuilding_WaterNet thing)
        {
            thing.OutputWaterNet = this;

            this.AddThingToList(thing);
        }

        public void RemoveThing(IBuilding_WaterNet thing)
        {
            this.RemoveThingFromList(thing);
        }

        private void AddThingToList(IBuilding_WaterNet thing)
        {
            // 全ての物を追加
            allThings.Add(thing);

            // 入力系を仕分けして追加
            if (thing.InputComp != null)
            {
                if (thing.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet)
                {
                    // 水道網入力タイプは、この水道網からの入力を受ける場合のみ追加
                    if (thing.InputWaterNet == this)
                    {
                        this.inputterTypeDic[thing.InputComp.InputType].Add(thing);
                    }
                }
                else
                {
                    // 水道網入力タイプ以外は無条件で追加
                    this.inputterTypeDic[thing.InputComp.InputType].Add(thing);
                }
            }

            // 出力系を追加
            if (thing.OutputComp != null && thing.OutputWaterNet == this)
            {
                this.outputters.Add(thing);
            }

            // タンク系を追加
            if (thing.TankComp != null)
            {
                this.tanks.Add(thing);
            }
        }

        private void RemoveThingFromList(IBuilding_WaterNet thing)
        {
            // 全リストから削除
            this.allThings.Remove(thing);

            // 入力辞書から削除
            foreach (var item in this.inputterTypeDic)
            {
                item.Value.Remove(thing);
            }

            // 出力リストから削除
            this.outputters.Remove(thing);

            // タンクリストから削除
            this.tanks.Remove(thing);
        }

        public void ClearThings()
        {
            foreach (var thing in allThings)
            {
                thing.InputWaterNet = null;
                thing.OutputWaterNet = null;
            }

            allThings.Clear();
            foreach (var item in inputterTypeDic)
            {
                item.Value.Clear();
            }
            outputters.Clear();
        }

        // 仮
        public float StoredWaterVolume
        {
            get
            {
                List<IBuilding_WaterNet> tanks = allThings.FindAll((t) => t.TankComp != null);

                float sumStoredWaterVolume = 0.0f;
                foreach (var tank in tanks)
                {
                    sumStoredWaterVolume += tank.GetComp<CompWaterNetTank>().StoredWaterVolume;
                }

                return sumStoredWaterVolume;
            }
        }

        // 仮
        public void DrawWaterVolume(float amount)
        {
            float totalAmount = amount;

            while (totalAmount > 0.1f)
            {
                List<IBuilding_WaterNet> tanks = allThings.FindAll((t) =>
                {
                    return (t.TankComp != null) && (t.TankComp.StoredWaterVolume > 0.0f);
                });

                if (tanks.Count == 0)
                {
                    break;
                }

                float averageAmount = totalAmount / tanks.Count;
                foreach (var tank in tanks)
                {
                    totalAmount -= tank.TankComp.DrawWaterVolume(averageAmount);
                }
            }
        }

        public void AddWaterVolume(float amount)
        {
            float totalAmount = amount;

            while (totalAmount > 0.0f)
            {
                List<IBuilding_WaterNet> tanks = allThings.FindAll((t) =>
                {
                    return (t.TankComp != null) && (t.TankComp.AmountCanAccept > 0.0f) && (t.InputComp != null) && (t.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet);
                });

                if (tanks.Count == 0)
                {
                    break;
                }

                float averageWaterFlow = totalAmount / tanks.Count;
                foreach (var tank in tanks)
                {
                    totalAmount -= tank.GetComp<CompWaterNetTank>().AddWaterVolume(averageWaterFlow);
                }
            }
        }

        public void UpdateInputWaterFlow()
        {
            // 入力値をクリア
            foreach (var item in inputterTypeDic)
            {
                foreach (var t in item.Value)
                {
                    t.InputComp.InputWaterFlow = 0.0f;
                    t.InputComp.InputWaterType = WaterType.NoWater;
                }
            }

            // 雨入力の入力量を決定
            foreach (var t in inputterTypeDic[CompProperties_WaterNetInput.InputType.Rain])
            {
                // 建造物にどれだけ屋根がかぶっているかチェック
                if (!t.InputComp.IsActivated) continue;

                t.InputComp.InputWaterFlow = t.InputComp.MaxInputWaterFlow * this.Manager.map.weatherManager.RainRate * t.GetUnroofedPercent();
                if (t.InputComp.InputWaterFlow > 0.0f) t.InputComp.InputWaterType = WaterType.RawWater;
            }
            // 地下水入力タイプの入力量を決定
            foreach (var t in inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterPool])
            {
                if (!t.InputComp.IsActivated) continue;

                if (t.WaterPool != null && t.WaterPool.CurrentWaterVolume > 0f)
                {
                    // 地下水があれば入力あり
                    t.InputComp.InputWaterFlow = t.InputComp.MaxInputWaterFlow;
                    t.InputComp.InputWaterType = t.WaterPool.WaterType;
                }
            }
            // 地形入力タイプの入力量を決定
            foreach (var t in inputterTypeDic[CompProperties_WaterNetInput.InputType.Terrain])
            {
                if (!t.InputComp.IsActivated) continue;

                var building = t as Building;
                var terrainWaterType = this.Manager.map.terrainGrid.TerrainAt(building.Position).ToWaterType();
                if (t.InputComp.AcceptWaterTypes.Contains(terrainWaterType))
                {
                    // 入力可能水質と地形が合っていれば入力あり
                    t.InputComp.InputWaterFlow = t.InputComp.MaxInputWaterFlow;
                    t.InputComp.InputWaterType = terrainWaterType;
                }
            }

            // 水道網入力タイプの入力量を決定
            // 出力を各入力設備に割り振っていく
            WaterType outputWaterType = WaterType.NoWater;
            foreach (var outputter in outputters)
            {
                // 出力量ゼロの場合スキップ
                if (outputter.OutputComp.OutputWaterFlow <= 0f) continue;

                // 水道網内の水質を更新
                // 全ての出力水質のうち最低ランクの物にする
                // ここでは水道網全体の水質を決めるだけ
                outputWaterType = (WaterType)Mathf.Min((int)outputWaterType, (int)outputter.OutputComp.OutputWaterType);

                // 出力設備が現在出力可能な相手のリスト
                var effectiveInputters = inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterNet].Where((t) =>
                {
                    // 自分自身は除外
                    if (t == outputter) return false;

                    // 機能していないものは除外
                    if (!t.InputComp.IsActivated) return false;

                    // タンクが満タンであれば除外
                    if (t.TankComp != null && t.TankComp.AmountCanAccept <= 0.0f) return false;

                    // 出力水質が入力可能水質リストになければ除外
                    if (!t.InputComp.AcceptWaterTypes.Contains(outputter.OutputComp.OutputWaterType)) return false;

                    return true;
                });

                // 残り出力量
                float remainOutputWaterFlow = outputter.OutputComp.OutputWaterFlow;

                // 出力可能な相手のうち、定量入力を必要とするタイプを先に割り振る
                var constantInputters = effectiveInputters.Where((t) =>
                {
                    return t.InputComp.InputWaterFlowType == CompProperties_WaterNetInput.InputWaterFlowType.Constant;
                });
                foreach (var inputter in constantInputters)
                {
                    if (remainOutputWaterFlow >= inputter.InputComp.MaxInputWaterFlow)
                    {
                        // 残量がまだ足りているなら入力する
                        inputter.InputComp.InputWaterFlow = inputter.InputComp.MaxInputWaterFlow;
                        remainOutputWaterFlow -= inputter.InputComp.MaxInputWaterFlow;
                    }
                }

                // 出力可能な相手のうち、任意入力で良いタイプに残量を割り振る
                while (remainOutputWaterFlow > 0.0f)
                {
                    var anyInputters = effectiveInputters.Where((t) =>
                    {
                        // 入力が任意で良いもの以外は除外
                        if (t.InputComp.InputWaterFlowType != CompProperties_WaterNetInput.InputWaterFlowType.Any) return false;

                        // タンクがあり満タンになったものは除外
                        if (t.TankComp != null && t.TankComp.AmountCanAccept <= 0.0f) return false;

                        // 入力量が最大まで達している物は除外
                        if (t.InputComp.InputWaterFlow >= t.InputComp.MaxInputWaterFlow) return false;

                        return true;
                    });

                    // 有効な入力装置がなくなったら出力が余っていても終了
                    if (anyInputters.Count() == 0) break;

                    // 均等割り
                    float aveOutputWaterFlow = remainOutputWaterFlow / anyInputters.Count();
                    foreach (var inputter in anyInputters)
                    {
                        // 最大を超えない量を計算して入力量を増加
                        float actualInputWaterFlow = Mathf.Min(aveOutputWaterFlow, inputter.InputComp.MaxInputWaterFlow - inputter.InputComp.InputWaterFlow);
                        inputter.InputComp.InputWaterFlow += actualInputWaterFlow;
                        remainOutputWaterFlow -= actualInputWaterFlow;
                    }
                }
            }

            // 水道網を流れる水の水質更新(水道網のタンクの水質とは別)
            this.waterType = outputWaterType;

            // 水道網入力タイプの現在の入力水質更新
            foreach (var inputter in inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterNet])
            {
                if (inputter.InputComp.InputWaterFlow > 0f)
                {
                    inputter.InputComp.InputWaterType = this.waterType;
                }
            }

            //foreach (var t1 in allThings)
            //{
            //    //// 雨から入力するタイプ
            //    //if (t1.InputComp != null && t1.InputComp.InputType == CompProperties_WaterNetInput.InputType.Rain)
            //    //{
            //    //    // 建造物にどれだけ屋根がかぶっているかチェック
            //    //    t1.InputComp.InputWaterFlow = t1.InputComp.MaxInputWaterFlow * this.Manager.map.weatherManager.RainRate * t1.GetUnroofedPercent();

            //    //    if (t1.InputComp.InputWaterFlow > 0.0f) t1.InputComp.InputWaterType = WaterType.RawWater;
            //    //}

            //    //// 地下水から入力するタイプ
            //    //if (t1.InputComp != null && t1.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterPool)
            //    //{
            //    //    if (t1.WaterPool != null && t1.WaterPool.CurrentWaterVolume > 0f)
            //    //    {
            //    //        t1.InputComp.InputWaterFlow = t1.InputComp.MaxInputWaterFlow;
            //    //        t1.InputComp.InputWaterType = t1.WaterPool.WaterType;
            //    //    }
            //    //}

            //    //// 地形から入力するタイプ
            //    //if (t1.InputComp != null && t1.InputComp.InputType == CompProperties_WaterNetInput.InputType.Terrain)
            //    //{
            //    //    var building = t1 as Building;
            //    //    var terrainWaterType = this.Manager.map.terrainGrid.TerrainAt(building.Position).ToWaterType();
            //    //    if (t1.InputComp.AcceptWaterTypes.Contains(terrainWaterType))
            //    //    {
            //    //        t1.InputComp.InputWaterFlow = t1.InputComp.MaxInputWaterFlow;
            //    //        t1.InputComp.InputWaterType = terrainWaterType;
            //    //    }
            //    //}

            //    // ここから水道網出力タイプの処理
            //    // 各出力源の処理を、水道網内の水道網入力タイプに割り振っていく

            //    //// 出力機能が無ければスキップ
            //    //if (t1.OutputComp == null || t1.OutputComp.OutputWaterFlow == 0.0f || t1.OutputWaterNet != this)
            //    //{
            //    //    continue;
            //    //}

                
            //    //List<IBuilding_WaterNet> t2list = allThings.FindAll((t) =>
            //    //{
            //    //    bool isOK = (t != t1);
            //    //    if (isOK) isOK &= t.InputWaterNet == this;
            //    //    if (isOK) isOK &= (t.InputComp != null);
            //    //    if (isOK) isOK &= t.InputComp.IsActivated;
            //    //    if (isOK) isOK &= (t.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet);
            //    //    if (isOK) isOK &= ((t.TankComp == null) || (t.TankComp.AmountCanAccept > 0.0f));
            //    //    if (isOK) isOK &= (t.InputComp.AcceptWaterTypes.Contains(this.WaterType));
            //    //    return isOK;
            //    //});

            //    //List<IBuilding_WaterNet> t2list_constant = t2list.FindAll((t) =>
            //    //{
            //    //    return t.InputComp.InputWaterFlowType == CompProperties_WaterNetInput.InputWaterFlowType.Constant;
            //    //});

            //    //float outputWaterFlow = t1.OutputComp.OutputWaterFlow;
            //    //foreach (var t2 in t2list_constant)
            //    //{
            //    //    if (outputWaterFlow >= t2.InputComp.MaxInputWaterFlow)
            //    //    {
            //    //        t2.InputComp.InputWaterFlow = t2.InputComp.MaxInputWaterFlow;
            //    //        outputWaterFlow -= t2.InputComp.MaxInputWaterFlow;
            //    //    }
            //    //}

            //    // 余った出力を、入力値が任意で良い入力装置に割り振る
            //    while (outputWaterFlow > 0.0f)
            //    {
            //        List<IBuilding_WaterNet> t2list_any = t2list.FindAll((t) =>
            //        {
            //            bool isOK = ((t.TankComp == null) || (t.TankComp.AmountCanAccept > 0.0f));
            //            if (isOK) isOK &= (t.InputComp.InputWaterFlowType == CompProperties_WaterNetInput.InputWaterFlowType.Any);
            //            if (isOK) isOK &= (t.InputComp.InputWaterFlow < t.InputComp.MaxInputWaterFlow);
            //            return isOK;
            //        });

            //        if (t2list_any.Count == 0)
            //        {
            //            break;
            //        }

            //        float aveOutputWaterFlow = outputWaterFlow / t2list_any.Count;
            //        foreach (var t2 in t2list_any)
            //        {
            //            float actualInputWaterFlow = Mathf.Min(aveOutputWaterFlow, t2.InputComp.MaxInputWaterFlow - t2.InputComp.InputWaterFlow);
            //            t2.InputComp.InputWaterFlow += actualInputWaterFlow;
            //            outputWaterFlow -= actualInputWaterFlow;
            //        }
            //    }
            //}
        }

        public void UpdateWaterTank()
        {
            // タンクの貯水量変更
            foreach (var tank in tanks)
            {
                // 入力と出力の差分だけ更新する
                float inputWaterFlow = 0.0f;
                if (tank.InputComp != null) inputWaterFlow = tank.InputComp.InputWaterFlow;

                float outputWaterFlow = 0.0f;
                if (tank.OutputComp != null) outputWaterFlow = tank.OutputComp.OutputWaterFlow;

                float deltaWaterFlow = inputWaterFlow - outputWaterFlow;
                if (deltaWaterFlow > 0.0f)
                {
                    tank.TankComp.AddWaterVolume(deltaWaterFlow / 60000.0f);
                }
                else if (deltaWaterFlow < 0.0f)
                {
                    tank.TankComp.DrawWaterVolume(-deltaWaterFlow / 60000.0f);
                }

                // 水抜き
                if (tank.TankComp.IsDraining)
                {
                    tank.TankComp.DrawWaterVolume(tank.TankComp.DrainWaterFlow / 60000);
                }
            }

            // タンク内の水質更新

            // 雨入力のタンク
            foreach (var tank in tanks.Intersect(inputterTypeDic[CompProperties_WaterNetInput.InputType.Rain]))
            {
                if (tank.TankComp.StoredWaterVolume <= 0f)
                {
                    // 空っぽになったらクリア
                    tank.TankComp.StoredWaterType = WaterType.NoWater;
                }
                else if (tank.InputComp.InputWaterFlow > 0f)
                {
                    // 空ではなく、入力を受けている場合
                    //   ⇒現在の水質と生水のうち低い方になる
                    tank.TankComp.StoredWaterType = (WaterType)Mathf.Min((int)tank.TankComp.StoredWaterType, (int)WaterType.RawWater);
                }
            }

            // 地下水入力のタンク
            foreach (var tank in tanks.Intersect(inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterPool]))
            {
                if (tank.TankComp.StoredWaterVolume <= 0f)
                {
                    // 空っぽになったらクリア
                    tank.TankComp.StoredWaterType = WaterType.NoWater;
                }
                else if (tank.InputComp.InputWaterFlow > 0f)
                {
                    // 空ではなく、入力を受けている場合
                    //   ⇒現在の水質と地下水の水質のうち低い方になる
                    tank.TankComp.StoredWaterType = (WaterType)Mathf.Min((int)tank.TankComp.StoredWaterType, (int)tank.WaterPool.WaterType);
                }
            }

            // 地形入力のタンク
            foreach (var tank in tanks.Intersect(inputterTypeDic[CompProperties_WaterNetInput.InputType.Terrain]))
            {
                if (tank.TankComp.StoredWaterVolume <= 0f)
                {
                    // 空っぽになったらクリア
                    tank.TankComp.StoredWaterType = WaterType.NoWater;
                }
                else if (tank.InputComp.InputWaterFlow > 0f)
                {
                    // 空ではなく、入力を受けている場合
                    //   ⇒現在の水質と地形の水質のうち低い方になる
                    var building = tank as Building;
                    var terrainWaterType = this.Manager.map.terrainGrid.TerrainAt(building.Position).ToWaterType();
                    tank.TankComp.StoredWaterType = (WaterType)Mathf.Min((int)tank.TankComp.StoredWaterType, (int)terrainWaterType);
                }
            }

            // 水道網入力のタンク
            foreach (var tank in tanks.Intersect(inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterNet]))
            {
                if (tank.TankComp.StoredWaterVolume <= 0f)
                {
                    // 空っぽになったらクリア
                    tank.TankComp.StoredWaterType = WaterType.NoWater;
                }
                else if (tank.InputComp.InputWaterFlow > 0f)
                {
                    // 空ではなく、入力を受けている場合
                    //   ⇒現在の水質と水道網の水質のうち低い方になる
                    tank.TankComp.StoredWaterType = (WaterType)Mathf.Min((int)tank.TankComp.StoredWaterType, (int)this.WaterType);
                }
            }

            // 水道網全体のタンク水質(蛇口をひねったときに出てくる水の種類)を決める
            this.storedWaterType = WaterType.NoWater;
            foreach (var tank in tanks)
            {
                this.storedWaterType = (WaterType)Mathf.Min((int)this.storedWaterType, (int)tank.TankComp.StoredWaterType);
            }

            //List<IBuilding_WaterNet> waterNetTanks = allThings.FindAll((t) =>
            //{
            //    return t.TankComp != null && t.InputComp != null && t.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet;
            //});
            //foreach (var tank in waterNetTanks)
            //{
            //    if (tank.TankComp.StoredWaterVolume == 0.0f)
            //    {
            //        tank.TankComp.StoredWaterType = WaterType.NoWater;
            //    }
            //}

            //List<IBuilding_WaterNet> rainTanks = allThings.FindAll((t) =>
            //{
            //    return t.TankComp != null && t.InputComp != null && t.InputComp.InputType == CompProperties_WaterNetInput.InputType.Rain;
            //});
            //foreach (var tank in rainTanks)
            //{
            //    if (tank.TankComp.StoredWaterVolume == 0.0f)
            //    {
            //        tank.TankComp.StoredWaterType = WaterType.NoWater;
            //    }
            //    else
            //    {
            //        tank.TankComp.StoredWaterType = WaterType.RawWater;
            //    }
            //}

            //List<IBuilding_WaterNet> tanks = allThings.FindAll((t) =>
            //{
            //    return (t.TankComp != null);
            //});

        }

        //public void UpdateWaterType()
        //{
        //    WaterType curWaterType = WaterType.NoWater;

        //    // 水道網の全出力から、現在の水道網に流れる水の種類を決める
        //    foreach (var t in allThings)
        //    {
        //        if (t.OutputWaterNet != this) continue;
        //        if (t.OutputComp == null) continue;

        //        if (t.OutputComp.OutputWaterType != WaterType.NoWater)
        //        {
        //            if (curWaterType == WaterType.NoWater)
        //            {
        //                curWaterType = t.OutputComp.OutputWaterType;
        //            }
        //            else
        //            {
        //                curWaterType = (WaterType)Math.Min((int)t.OutputComp.OutputWaterType, (int)curWaterType);
        //            }
        //        }
        //    }

        //    // 入力の水質更新
        //    var inputters = allThings.FindAll((t) => t.InputComp != null && t.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet && t.InputWaterNet == this);
        //    foreach (var inputter in inputters)
        //    {
        //        if (inputter.InputComp.InputWaterFlow > 0f)
        //        {
        //            inputter.InputComp.InputWaterType = curWaterType;
        //        }
        //    }

        //    // タンクの水質の更新
        //    List<IBuilding_WaterNet> tanks = allThings.FindAll((t) => t.GetComp<CompWaterNetTank>() != null);
        //    if (curWaterType != WaterType.NoWater)
        //    {
        //        this.waterType = curWaterType;
        //        foreach (var tank in tanks)
        //        {
        //            if (tank.InputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet)
        //            {
        //                tank.TankComp.StoredWaterType = curWaterType;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var tank in tanks)
        //        {
        //            WaterType tankWaterType = tank.TankComp.StoredWaterType;
        //            if (tankWaterType != WaterType.NoWater)
        //            {
        //                if (curWaterType == WaterType.NoWater)
        //                {
        //                    curWaterType = tank.TankComp.StoredWaterType;
        //                }
        //                else
        //                {
        //                    curWaterType = (WaterType)Math.Min((int)tank.TankComp.StoredWaterType, (int)curWaterType);
        //                }
        //            }
        //            this.waterType = curWaterType;
        //        }
        //    }

        //}
    }
}
