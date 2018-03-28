using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Verse;

namespace MizuMod
{
    public class MapComponent_WaterNetManager : MapComponent
    {
        private bool requestedUpdateWaterNet = false;

        private List<WaterNet> nets = new List<WaterNet>();
        public List<WaterNet> Nets
        {
            get
            {
                return nets;
            }
        }

        private List<IBuilding_WaterNet> unNetThings = new List<IBuilding_WaterNet>();
        public List<IBuilding_WaterNet> UnNetThings
        {
            get
            {
                return unNetThings;
            }
        }

        public MapComponent_WaterNetManager(Map map) : base(map)
        {
        }

        public void RequestUpdateWaterNet()
        {
            this.requestedUpdateWaterNet = true;
        }

        public Queue<IBuilding_WaterNet> ClearWaterNets()
        {
            Queue<IBuilding_WaterNet> unNetQueue = new Queue<IBuilding_WaterNet>();

            foreach (var t in unNetThings)
            {
                t.InputWaterNet = null;
                t.OutputWaterNet = null;
                if (!unNetQueue.Contains(t))
                {
                    unNetQueue.Enqueue(t);
                }
            }
            this.unNetThings.Clear();

            foreach (var net in nets)
            {
                foreach (var t in net.AllThings)
                {
                    if (!unNetQueue.Contains(t))
                    {
                        t.InputWaterNet = null;
                        t.OutputWaterNet = null;
                        unNetQueue.Enqueue(t);
                    }
                }
                net.ClearThings();
            }
            this.ClearNets();

            WaterNet.ClearNextID();

            return unNetQueue;
        }

        public void UpdateWaterNets()
        {
            Queue<IBuilding_WaterNet> unNetQueue = this.ClearWaterNets();
            Queue<IBuilding_WaterNet> unNetDiffQueue = new Queue<IBuilding_WaterNet>();

            while (unNetQueue.Count > 0)
            {
                IBuilding_WaterNet thing = unNetQueue.Dequeue();
                List<WaterNet> inputNets = new List<WaterNet>();
                List<WaterNet> outputNets = new List<WaterNet>();

                if (!thing.IsSameConnector)
                {
                    unNetDiffQueue.Enqueue(thing);
                    continue;
                }

                if (!thing.HasConnector)
                {
                    this.unNetThings.Add(thing);
                    continue;
                }
                foreach (var net in nets)
                {
                    foreach (var t in net.AllThings)
                    {
                        if (thing.IsOutputTo(t) && !outputNets.Contains(net))
                        {
                            outputNets.Add(t.InputWaterNet);
                        }
                        if (t.IsOutputTo(thing) && !inputNets.Contains(net))
                        {
                            inputNets.Add(t.OutputWaterNet);
                        }
                    }
                }

                List<WaterNet> connectNets = new List<WaterNet>();
                connectNets.AddRange(inputNets);
                foreach (var net in outputNets)
                {
                    if (!connectNets.Contains(net))
                    {
                        connectNets.Add(net);
                    }
                }

                if (connectNets.Count == 0)
                {
                    // 0個=新しい水道網
                    WaterNet newNet = new WaterNet();
                    newNet.AddThing(thing);
                    this.AddNet(newNet);
                }
                else if (connectNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    if (!connectNets[0].AllThings.Contains(thing))
                    {
                        connectNets[0].AddThing(thing);
                    }
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    if (!connectNets[0].AllThings.Contains(thing))
                    {
                        connectNets[0].AddThing(thing);
                    }
                    for (int i = 1; i < connectNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in connectNets[i].AllThings)
                        {
                            if (!connectNets[0].AllThings.Contains(t))
                            {
                                connectNets[0].AddThing(t);
                            }
                        }

                        // 接続水道網の終えたので水道網を削除
                        nets.Remove(connectNets[i]);
                    }
                }
            }

            while (unNetDiffQueue.Count > 0)
            {
                IBuilding_WaterNet thing = unNetDiffQueue.Dequeue();
                List<WaterNet> inputNets = new List<WaterNet>();
                List<WaterNet> outputNets = new List<WaterNet>();

                if (!thing.HasConnector)
                {
                    this.unNetThings.Add(thing);
                    continue;
                }
                foreach (var net in nets)
                {
                    foreach (var t in net.AllThings)
                    {
                        if (thing.IsOutputTo(t) && !outputNets.Contains(net))
                        {
                            outputNets.Add(t.InputWaterNet);
                        }
                        if (t.IsOutputTo(thing) && !inputNets.Contains(net))
                        {
                            inputNets.Add(t.OutputWaterNet);
                        }
                    }
                }

                if (inputNets.Count == 0)
                {
                    // 0個=新しい水道網
                    WaterNet newNet = new WaterNet();
                    newNet.AddInputThing(thing);
                    this.AddNet(newNet);
                }
                else if (inputNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    if (!inputNets[0].AllThings.Contains(thing))
                    {
                        inputNets[0].AddInputThing(thing);
                    }
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    if (!inputNets[0].AllThings.Contains(thing))
                    {
                        inputNets[0].AddInputThing(thing);
                    }
                    for (int i = 1; i<inputNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in inputNets[i].AllThings)
                        {
                            if (!inputNets[0].AllThings.Contains(t))
                            {
                                inputNets[0].AddInputThing(t);
                            }
                        }

                        // 接続水道網の終えたので水道網を削除
                        nets.Remove(inputNets[i]);
                    }
                }

                if (outputNets.Count == 0)
                {
                    // 0個=新しい水道網
                    WaterNet newNet = new WaterNet();
                    newNet.AddOutputThing(thing);
                    this.AddNet(newNet);
                }
                else if (outputNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    if (!outputNets[0].AllThings.Contains(thing))
                    {
                        outputNets[0].AddOutputThing(thing);
                    }
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    if (!outputNets[0].AllThings.Contains(thing))
                    {
                        outputNets[0].AddOutputThing(thing);
                    }
                    for (int i = 1; i<outputNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in outputNets[i].AllThings)
                        {
                            if (!outputNets[0].AllThings.Contains(t))
                            {
                                outputNets[0].AddOutputThing(t);
                            }
                        }

                        // 接続水道網の終えたので水道網を削除
                        nets.Remove(outputNets[i]);
                    }
                }

            }
        }

        public void AddThing(IBuilding_WaterNet thing)
        {
            this.unNetThings.Add(thing);
            this.UpdateWaterNets();
        }

        public void RemoveThing(IBuilding_WaterNet thing)
        {
            // 対象の物を除去
            if (thing.InputWaterNet != null)
            {
                thing.InputWaterNet.RemoveThing(thing);
            }
            if (thing.OutputWaterNet != null)
            {
                thing.OutputWaterNet.RemoveThing(thing);
            }
            if (this.unNetThings.Contains(thing))
            {
                this.unNetThings.Remove(thing);
            }

            this.UpdateWaterNets();
        }

        public void AddNet(WaterNet net)
        {
            net.Manager = this;
            this.nets.Add(net);
        }

        public void RemoveNet(WaterNet net)
        {
            net.Manager = null;
            this.nets.Remove(net);
        }

        public void ClearNets()
        {
            foreach (var net in nets)
            {
                net.Manager = null;
            }
            nets.Clear();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (this.requestedUpdateWaterNet)
            {
                this.requestedUpdateWaterNet = false;
                this.UpdateWaterNets();
            }

            // 入力量と入力水質、水道網全体の水質を更新
            foreach (var net in nets)
            {
                net.UpdateInputWaterFlow();
            }
            // タンクの水量とタンク内の水質を更新
            foreach (var net in nets)
            {
                net.UpdateWaterTank();
            }
        }
    }
}
