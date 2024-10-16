using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace modtest1
{
    class SpriteBound
    {

        string name;
        Rect rect;
        Vector2[] vertices;
        ushort[] triangles;

        public SpriteBound(string n, string r, string v)
        {
            name = n;
            rect = MakeRect(r);
            vertices = MakeVertices(v);
            triangles = MakeTriangles();
        }

        private Rect MakeRect(string r)
        {
            var list = r.Split(',');
            var ints = new List<int>();
            foreach (var v in list)
                ints.Add(int.Parse(v));
            return new Rect(ints[0], ints[1], ints[2], ints[3]);
        }

        private Vector2[] MakeVertices(string v)
        {
            var list = v.Split(',');
            Vector2[] vecs = new Vector2[(int) list.Length/2];
            vecs[0] = new Vector2(rect.x + rect.width/2, rect.y + rect.height/2);
            for (int i = 0; i < list.Length; i = i + 2)
                vecs[(int) i/2] = new Vector2(int.Parse(list[i]), int.Parse(list[i+1]));
            return vecs;
        }

        private ushort[] MakeTriangles()
        {
            ushort[] triangles = new ushort[(vertices.Length - 1) * 3];
            for (ushort i = 0; i < vertices.Length - 1; i++)
            {
                triangles[i*3] = 0;
                triangles[i*3 + 1] = Convert.ToUInt16((i + 1) % vertices.Length);
                triangles[i*3 + 2] = Convert.ToUInt16((i + 2) % vertices.Length);
            }
            //triangles[triangles.Length-3] = 0;
            //triangles[triangles.Length-2] = Convert.ToUInt16(vertices.Length-1);
            //triangles[triangles.Length-1] = 1;
            return triangles;
        }

        public static void ReplaceSprite(List<SpriteBound> spriteBounds, Sprite sprite)
        {
                var bound = spriteBounds[GetBoundIndexByName(spriteBounds, sprite.name)];

                var s = Sprite.Create(sprite.texture, bound.rect, new Vector2(0.5f, 0.5f));
                s.OverrideGeometry(bound.vertices, bound.triangles);
                sprite = s;
        }

        public static int GetBoundIndexByName(List<SpriteBound> sbs, string name)
        {
            for (int i = 0; i < sbs.Count; i++)
                if (sbs[i].name == name)
                    return i;
            return -1;
        }
    }
}
