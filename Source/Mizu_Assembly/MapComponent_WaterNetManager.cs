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

        public void AddThing(ThingWithComps thing)
        {
            List<WaterNet> connectNet = new List<WaterNet>();

            // 各水道網に所属している物の中に、新しく生成した物の4近傍に配置されているものがあるか探す
            foreach (var net in nets)
            {
                foreach (var t in net.Things)
                {
                    if (thing.IsAdjacentToCardinalOrInside(t) && !connectNet.Contains(net))
                    {
                        connectNet.Add(net);
                        break;
                    }
                }
            }

            if (connectNet.Count == 0)
            {
                // 0個=新しい水道網
                WaterNet net = new WaterNet();
                nets.Add(net);
                net.AddThing(thing);
            }
            else if (connectNet.Count == 1)
            {
                // 1個=既存の水道網に加える
                connectNet[0].AddThing(thing);
            }
            else
            {
                // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                connectNet[0].AddThing(thing);
                for (int i = 1; i < connectNet.Count; i++)
                {
                    foreach (var t in connectNet[i].Things)
                    {
                        t.GetComp<CompWaterNet>().WaterNet = connectNet[0];
                    }
                    connectNet[0].Things.AddRange(connectNet[i].Things);
                    nets.Remove(connectNet[i]);
                }
            }
        }

        public void RemoveThing(ThingWithComps thing)
        {
            WaterNet curNet = thing.GetComp<CompWaterNet>().WaterNet;

            // 対象の物を除去
            curNet.RemoveThing(thing);
            thing.GetComp<CompWaterNet>().WaterNet = null;

            // 対象が所属していた水道網を水道網リストから除去
            nets.Remove(curNet);

            // 対象が所属していた水道網内の残りの物を一旦未所属にする
            List<ThingWithComps> unNetThings = new List<ThingWithComps>(curNet.Things);
            foreach (var t in unNetThings)
            {
                t.GetComp<CompWaterNet>().WaterNet = null;
            }
            curNet.Things.Clear();

            DecideWaterNet(unNetThings);
        }

        private void DecideWaterNet(List<ThingWithComps> unNetThings)
        {
            while (unNetThings.Count > 0)
            {
                // 未所属のうち1個を選んで新しい水道網に所属させる
                ThingWithComps firstThing = unNetThings[0];
                unNetThings.Remove(firstThing);
                WaterNet newNet = new WaterNet();
                nets.Add(newNet);
                firstThing.GetComp<CompWaterNet>().WaterNet = newNet;
                newNet.AddThing(firstThing);

                // 選んだ1個を、新しい水道網と接続されているかどうか探索する起点にする
                List<ThingWithComps> searchThings = new List<ThingWithComps>();
                searchThings.Add(firstThing);

                while (searchThings.Count > 0)
                {
                    // 起点の1個を選ぶ
                    ThingWithComps searchThing = searchThings[0];
                    searchThings.Remove(searchThing);

                    // 起点の4近傍にある未所属の物を探す
                    List<ThingWithComps> neighborThings = FindNeighborUnNetThing(searchThing, unNetThings);
                    foreach (var t in neighborThings)
                    {
                        // リストに加え、新しい探索起点とする
                        t.GetComp<CompWaterNet>().WaterNet = newNet;
                        newNet.AddThing(t);
                        searchThings.Add(t);

                        // 未所属リストからは外す
                        unNetThings.Remove(t);
                    }
                }
            }
        }

        private List<ThingWithComps> FindNeighborUnNetThing(ThingWithComps thing, List<ThingWithComps> unNetThings)
        {
            List<ThingWithComps> neighborThings = new List<ThingWithComps>();

            foreach (var t in unNetThings)
            {
                if (!neighborThings.Contains(t) && thing.IsAdjacentToCardinalOrInside(t))
                {
                    neighborThings.Add(t);
                }
            }
            return neighborThings;
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
