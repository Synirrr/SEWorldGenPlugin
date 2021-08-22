﻿using Sandbox.Definitions;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// Sub menu for the star system designer to edit or spawn planets
    /// </summary>
    public class MyStarSystemDesignerPlanetMenu : MyStarSystemDesignerObjectMenu
    {
        /// <summary>
        /// A combobox for the type of planet
        /// </summary>
        MyGuiControlCombobox m_planetTypeCombobox;

        /// <summary>
        /// A slider for the size of the planet in m
        /// </summary>
        MyGuiControlClickableSlider m_sizeSlider;

        /// <summary>
        /// A textbox to enter the orbit radius in km
        /// </summary>
        MyGuiControlTextbox m_orbitRadiusTextbox;

        /// <summary>
        /// A slider for the position of the planet on the orbit, between 0 and 360 degrees.
        /// </summary>
        MyGuiControlClickableSlider m_orbitPosSlider;

        /// <summary>
        /// The slider for the elevation of the planet over the system plane, between -90 and 90 degrees.
        /// </summary>
        MyGuiControlClickableSlider m_elevationSldier;

        /// <summary>
        /// A list of planet types to spawn
        /// </summary>
        List<MyPlanetGeneratorDefinition> m_planetTypes;

        public MyStarSystemDesignerPlanetMenu(MySystemObject obj) : base(obj)
        {
            if(!(obj is MySystemPlanet) || obj == null)
            {
                MyPluginLog.Log("Trying to edit a non planet with the planet editing menu.", LogLevel.ERROR);
                m_object = new MySystemPlanet();
            }
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            var settings = MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings;
            m_planetTypeCombobox = new MyGuiControlCombobox();
            m_planetTypeCombobox.Size = new VRageMath.Vector2(maxWidth, m_planetTypeCombobox.Size.Y);
            m_planetTypeCombobox.Tooltips.AddToolTip("The type of the planet.");
            m_planetTypeCombobox.ItemSelected += OnTypeSelected;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Planet type"));
            controlTable.AddTableRow(m_planetTypeCombobox);

            m_sizeSlider = new MyGuiControlClickableSlider(null, 1f, settings.PlanetSizeCap, maxWidth - 0.1f, intValue: true, labelSuffix: " m");
            m_sizeSlider.Tooltips.AddToolTip("The size of the planet in meters.");
            m_sizeSlider.ValueChanged += OnSizeChanged;

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Planet size"));
            controlTable.AddTableRow(m_sizeSlider);

            m_orbitRadiusTextbox = new MyGuiControlTextbox(type: MyGuiControlTextboxType.DigitsOnly);
            m_orbitRadiusTextbox.Size = new VRageMath.Vector2(maxWidth - 0.1f, m_orbitRadiusTextbox.Size.Y);
            m_orbitRadiusTextbox.Tooltips.AddToolTip("The radius of the planets orbit. Its the distance of the planets center to the system center.");
            m_orbitRadiusTextbox.TextChanged += (tb) =>
            {
                GetPropertiesFromOrbit();
            };

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Orbit radius"));
            controlTable.AddTableRow(m_orbitRadiusTextbox);

            m_orbitPosSlider = new MyGuiControlClickableSlider(null, 0f, 360f, maxWidth - 0.1f, 0f, showLabel: true);
            m_orbitPosSlider.Tooltips.AddToolTip("The position of the planet on the orbit itself. Moves the planet around on the orbit.");
            m_orbitPosSlider.ValueChanged += (s) =>
            {
                GetPropertiesFromOrbit();
            };

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Planet position"));
            controlTable.AddTableRow(m_orbitPosSlider);

            m_elevationSldier = new MyGuiControlClickableSlider(null, -90f, 90f, maxWidth - 0.1f, 0f);
            m_elevationSldier.Tooltips.AddToolTip("The elevation of the planets orbit above the system plane.");
            m_elevationSldier.ValueChanged += (s) =>
            {
                GetPropertiesFromOrbit();
            };

            controlTable.AddTableRow(new MyGuiControlLabel(text: "Elevation degrees"));
            controlTable.AddTableRow(m_elevationSldier);

            LoadPlanetList();
            LoadPlanetProperties();

            if (isEditing)
            {
                m_planetTypeCombobox.Enabled = false;
                m_sizeSlider.Enabled = false;
                m_orbitRadiusTextbox.Enabled = false;
                m_orbitPosSlider.Enabled = false;
                m_elevationSldier.Enabled = false;
            }
        }

        /// <summary>
        /// Loads the properties from the edited object and sets the sliders accordingly.
        /// </summary>
        private void LoadPlanetProperties()
        {
            if (m_object == null) return;
            SetPlanetType();
            SetPlanetSize();
            SetOrbitProperties();
        }

        /// <summary>
        /// Gets the planet type from the edited object and sets the selected item in the combobox.
        /// </summary>
        private void SetPlanetType()
        {
            MySystemPlanet planet = m_object as MySystemPlanet;

            string type = planet.SubtypeId;

            foreach(var planetDef in m_planetTypes)
            {
                if(planetDef.Id.SubtypeId.ToString() == type)
                {
                    m_planetTypeCombobox.SelectItemByKey(m_planetTypes.IndexOf(planetDef));
                    return;
                }
            }
        }

        /// <summary>
        /// Action when planet type got selected.
        /// </summary>
        private void OnTypeSelected()
        {
            int typeIndex = (int)m_planetTypeCombobox.GetSelectedKey();
            MySystemPlanet p = m_object as MySystemPlanet;
            p.SubtypeId = m_planetTypes[typeIndex].Id.SubtypeId.ToString();
        }

        /// <summary>
        /// Sets the slider for planet size to the size of the edited planet
        /// </summary>
        private void SetPlanetSize()
        {
            MySystemPlanet planet = m_object as MySystemPlanet;
            m_sizeSlider.Value = (float)planet.Diameter;
        }

        /// <summary>
        /// Action when the size of the object is changed
        /// </summary>
        /// <param name="s">Slider for size</param>
        private void OnSizeChanged(MyGuiControlSlider s)
        {
            MySystemPlanet planet = m_object as MySystemPlanet;
            planet.Diameter = s.Value;
        }

        /// <summary>
        /// Sets the sliders for orbit radius, orbit position and elevation from the edited object.
        /// </summary>
        private void SetOrbitProperties()
        {
            double radius = new Vector3D(m_object.CenterPosition).Length();
            double elevation = Math.Asin(m_object.CenterPosition.z / radius) * (180.0 / Math.PI);
            double orbitPos = Math.Asin(m_object.CenterPosition.x / Math.Cos(elevation) / radius) * (180.0 / Math.PI);
            if(orbitPos < 0)
            {
                orbitPos += 360;
            }

            var radSB = new StringBuilder();
            radSB.Append(radius);

            m_orbitRadiusTextbox.SetText(radSB);
            m_elevationSldier.Value = (float)elevation;
            m_orbitPosSlider.Value = (float)orbitPos;
        }

        /// <summary>
        /// Sets the m_object properties based on the current values for the orbit radius, orbit position and elevation controls
        /// </summary>
        private void GetPropertiesFromOrbit()
        {
            var radSB = new StringBuilder();
            m_orbitRadiusTextbox.GetText(radSB);

            if (!double.TryParse(radSB.ToString(), out double radius)) return;

            double elevation = m_elevationSldier.Value;
            double orbitPos = m_orbitPosSlider.Value;

            Vector3D pos = new Vector3D(radius * Math.Sin(orbitPos) * Math.Cos(elevation), radius * Math.Cos(orbitPos) * Math.Cos(elevation), radius * Math.Sin(elevation));

            m_object.CenterPosition = pos;
        }

        /// <summary>
        /// Loads all planet types and puts them into the PlanetTypeCombobox aswell as the planetTypes list for index association.
        /// </summary>
        private void LoadPlanetList()
        {
            m_planetTypeCombobox.Clear();
            m_planetTypes.Clear();
            var planets = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions();

            foreach(var planet in planets)
            {
                m_planetTypes.Add(planet);
                m_planetTypeCombobox.AddItem(m_planetTypes.IndexOf(planet), planet.Id.SubtypeId.ToString());
            }
        }
    }
}
