using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Verse;

namespace MizuMod
{
    public class MapComponent_WaterNetManager : MapComponent
    {
        private List<WaterNet> nets = new List<WaterNet>();
        private List<IBuilding_WaterNet> unNetThings = new List<IBuilding_WaterNet>();

        public List<WaterNet> Nets
        {
            get
            {
                return nets;
            }
        }

        public MapComponent_WaterNetManager(Map map) : base(map)
        {
        }

        public Queue<IBuilding_WaterNet> ClearWaterNets()
        {
            Queue<IBuilding_WaterNet> unNetQueue = new Queue<IBuilding_WaterNet>();

            foreach (var net in nets)
            {
                foreach (var t in net.Things)
                {
                    unNetQueue.Enqueue(t);
                }
                net.ClearThings();
            }
            this.ClearNets();

            foreach (var t in this.unNetThings)
            {
                unNetQueue.Enqueue(t);
            }
            this.unNetThings.Clear();

            WaterNet.ClearNextID();

            return unNetQueue;
        }

        public void RefreshWaterNets()
        {
            Queue<IBuilding_WaterNet> unNetQueue = this.ClearWaterNets();

            while (unNetQueue.Count > 0)
            {
                IBuilding_WaterNet thing = unNetQueue.Dequeue();
                List<WaterNet> connectNets = new List<WaterNet>();

                if (!thing.HasConnector)
                {
                    this.unNetThings.Add(thing);
                    continue;
                }

                foreach (var net in nets)
                {
                    foreach (var t in net.Things)
                    {
                        if (thing.IsConnectedTo(t) && !connectNets.Contains(net))
                        {
                            connectNets.Add(net);
                            break;
                        }
                    }
                }

                if (connectNets.Count == 0)
                {
                    // 0個=新しい水道網
                    this.AddNet(new WaterNet(thing));
                }
                else if (connectNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    connectNets[0].AddThing(thing);
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    connectNets[0].AddThing(thing);
                    for (int i = 1; i < connectNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in connectNets[i].Things)
                        {
                            connectNets[0].AddThing(t);
                        }

                        // 接続水道網の終えたので水道網を削除
                        nets.Remove(connectNets[i]);
                    }
                }
            }

            foreach (var net in nets)
            {
                net.UpdateWaterType();
            }
        }

        public void AddThing(IBuilding_WaterNet thing)
        {
            if (nets.Count == 0)
            {
                nets.Add(new WaterNet(thing));
                return;
            }

            nets[0].Things.Add(thing);
            this.RefreshWaterNets();
        }

        public void RemoveThing(IBuilding_WaterNet thing)
        {
            // 対象の物を除去
            thing.WaterNet.RemoveThing(thing);

            this.RefreshWaterNets();
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

            foreach (var net in nets)
            {
                net.UpdateOutputWaterFlow();
            }
            foreach (var net in nets)
            {
                net.UpdateInputWaterFlow();
            }
            foreach (var net in nets)
            {
                net.UpdateWaterTankStorage();
            }
            foreach (var net in nets)
            {
                net.UpdateWaterType();
            }
        }
    }
}
