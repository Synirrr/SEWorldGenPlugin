﻿using Sandbox.Engine.Utils;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// A class that is used to render a planet and its orbit into the scene
    /// </summary>
    public class MyPlanetOrbitRenderObject : MyOrbitRenderObject
    {
        /// <summary>
        /// The render object used to render the planets sphere
        /// </summary>
        private RenderSphere m_planetRender;

        public MyPlanetOrbitRenderObject(MySystemPlanet planet) : base(planet)
        {
            m_planetRender = new RenderSphere(planet.CenterPosition, (float)planet.Diameter / 2, Color.DarkGreen.ToVector4());
        }

        public override void Draw()
        {
            base.Draw();
            UpdatePlanetSphereSize();

            m_planetRender.Color = IsFocused ? Color.LightBlue.ToVector4() : Color.DarkGreen.ToVector4();
            m_planetRender.Position = RenderObject.CenterPosition;
            m_planetRender.Draw();
        }

        /// <summary>
        /// Updates the render sphere size to be appropriatly sized
        /// </summary>
        private void UpdatePlanetSphereSize()
        {
            var specPos = MySpectatorCameraController.Static.Position;
            double distance = Vector3D.Distance(RenderObject.CenterPosition, specPos);
            var planet = RenderObject as MySystemPlanet;

            double multiplier = distance / 5000000f;

            double size = planet.Diameter * multiplier;

            if (multiplier > 100)
            {
                size = planet.Diameter * 100;
            }
            else if(size < planet.Diameter / 2f)
            {
                size = (float)(planet.Diameter / 2f);
            }

            m_planetRender.Radius = (float)size;
        }

        public override double GetObjectSize()
        {
            return MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.PlanetSizeCap / 2;
        }
    }
}
