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
        private List<ThingWithComps> unNetThings = new List<ThingWithComps>();

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

        public Queue<ThingWithComps> ClearWaterNets()
        {
            Queue<ThingWithComps> unNetQueue = new Queue<ThingWithComps>();

            foreach (var net in nets)
            {
                foreach (var t in net.Things)
                {
                    t.GetComp<CompWaterNetBase>().WaterNet = null;
                    unNetQueue.Enqueue(t);
                }
                net.Things.Clear();
            }
            nets.Clear();

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
            Queue<ThingWithComps> unNetQueue = this.ClearWaterNets();

            while (unNetQueue.Count > 0)
            {
                ThingWithComps thing = unNetQueue.Dequeue();
                List<WaterNet> connectNets = new List<WaterNet>();

                Building_Valve valve = thing as Building_Valve;
                if (valve != null && !valve.IsOpen)
                {
                    this.unNetThings.Add(valve);
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
                    WaterNet newNet = new WaterNet();
                    newNet.AddThing(thing);
                    nets.Add(newNet);
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
        }

        public void AddThing(ThingWithComps thing)
        {
            if (nets.Count == 0)
            {
                WaterNet newNet = new WaterNet();
                newNet.AddThing(thing);
                nets.Add(newNet);
                return;
            }

            nets[0].Things.Add(thing);
            this.RefreshWaterNets();
        }

        public void RemoveThing(ThingWithComps thing)
        {
            WaterNet curNet = thing.GetComp<CompWaterNet>().WaterNet;

            // 対象の物を除去
            curNet.RemoveThing(thing);
            thing.GetComp<CompWaterNet>().WaterNet = null;

            this.RefreshWaterNets();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            foreach (var net in nets)
            {
                net.Tick();
            }
        }
    }
}
