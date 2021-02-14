using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing
{
    public class MyAsteroidRingAdminMenu : IMyAsteroidAdminMenuCreator
    {
        private static int PREVIEW_RENDER_ID = 1425;

        /// <summary>
        /// The current star system existend on the server used for spawning rings around planets
        /// </summary>
        private MyObjectBuilder_SystemData m_fetchedStarSytem;

        private MyGuiControlListbox m_parentObjectListBox;

        private MyGuiControlClickableSlider m_radiusSlider;
        private MyGuiControlClickableSlider m_widthSlider;
        private MyGuiControlRangedSlider m_asteroidSizesSlider;
        private MyGuiControlClickableSlider m_angleXSlider;
        private MyGuiControlClickableSlider m_angleYSlider;
        private MyGuiControlClickableSlider m_angleZSlider;
        private MyGuiControlTextbox m_nameBox;

        private MySystemAsteroids m_currentSelectedAsteroid;

        private MyPluginAdminMenu m_parentScreen;

        public bool CreateEditMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen, MySystemAsteroids asteroidObject)
        {
            m_parentScreen = adminScreen;
            m_currentSelectedAsteroid = asteroidObject;

            MyGuiControlButton teleportToRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Teleport to ring", OnTeleportToRing);

            parentTable.AddTableRow(teleportToRingButton);

            MyGuiControlButton deleteRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Remove ring", OnRemoveRing);

            parentTable.AddTableRow(deleteRingButton);

            parentTable.AddTableSeparator();

            return true;
        }

        public bool CreateSpawnMenu(float usableWidth, MyGuiControlParentTableLayout parentTable, MyPluginAdminMenu adminScreen)
        {
            m_parentScreen = adminScreen;

            if (m_fetchedStarSytem == null)
            {
                MyGuiControlRotatingWheel m_loadingWheel = new MyGuiControlRotatingWheel(position: Vector2.Zero);
                m_loadingWheel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;

                adminScreen.Controls.Add(m_loadingWheel);

                MyStarSystemGenerator.Static.GetStarSystem(delegate (MyObjectBuilder_SystemData starSystem)
                {
                    m_fetchedStarSytem = starSystem;
                    adminScreen.ShouldRecreate = true;
                });
                return true;
            }

            MyGuiControlLabel label = new MyGuiControlLabel(null, null, "Parent objects");

            parentTable.AddTableRow(label);

            m_parentObjectListBox = new MyGuiControlListbox();
            m_parentObjectListBox.Add(new MyGuiControlListbox.Item(new System.Text.StringBuilder("System center"), userData: m_fetchedStarSytem.CenterObject));
            m_parentObjectListBox.VisibleRowsCount = 8;
            m_parentObjectListBox.Size = new Vector2(usableWidth, m_parentObjectListBox.Size.Y);
            m_parentObjectListBox.SelectAllVisible();
            m_parentObjectListBox.ItemsSelected += OnParentItemClicked;

            foreach(var obj in m_fetchedStarSytem.CenterObject.GetAllChildren())
            {
                if(obj.Type == MySystemObjectType.PLANET || obj.Type == MySystemObjectType.MOON)
                {
                    m_parentObjectListBox.Add(new MyGuiControlListbox.Item(new System.Text.StringBuilder(obj.DisplayName), userData: obj));
                }
            }

            parentTable.AddTableRow(m_parentObjectListBox);

            m_radiusSlider = new MyGuiControlClickableSlider(width: usableWidth - 0.1f, minValue: 0, maxValue: 1, labelSuffix: " km", showLabel: true);
            m_radiusSlider.Enabled = false;
            m_radiusSlider.ValueChanged += delegate
            {
                UpdateRingVisual();
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Radius"));
            parentTable.AddTableRow(m_radiusSlider);

            m_widthSlider = new MyGuiControlClickableSlider(null, 0, 1, usableWidth - 0.1f, 0.5f, showLabel: true, labelSuffix: " km");
            m_widthSlider.Enabled = false;
            m_widthSlider.ValueChanged += delegate
            {
                UpdateRingVisual();
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Width"));
            parentTable.AddTableRow(m_widthSlider);

            m_asteroidSizesSlider = new MyGuiControlRangedSlider(32, 1024, 32, 1024, true, width: usableWidth - 0.1f, showLabel: true);
            m_asteroidSizesSlider.Enabled = false;

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Asteroid size range"));
            parentTable.AddTableRow(m_asteroidSizesSlider);

            m_angleXSlider = new MyGuiControlClickableSlider(null, -90, 90, usableWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "�");
            m_angleXSlider.Enabled = false;
            m_angleXSlider.ValueChanged += delegate
            {
                UpdateRingVisual();
            };

            m_angleYSlider = new MyGuiControlClickableSlider(null, -90, 90, usableWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "�");
            m_angleYSlider.Enabled = false;
            m_angleYSlider.ValueChanged += delegate
            {
                UpdateRingVisual();
            };

            m_angleZSlider = new MyGuiControlClickableSlider(null, -90, 90, usableWidth - 0.1f, defaultValue: 0, intValue: true, showLabel: true, labelSuffix: "�");
            m_angleZSlider.Enabled = false;
            m_angleZSlider.ValueChanged += delegate
            {
                UpdateRingVisual();
            };

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Angle X Axis"));
            parentTable.AddTableRow(m_angleXSlider);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Angle Y Axis"));
            parentTable.AddTableRow(m_angleYSlider);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Angle Z Axis"));
            parentTable.AddTableRow(m_angleZSlider);

            m_nameBox = new MyGuiControlTextbox();
            m_nameBox.Size = new Vector2(usableWidth, m_nameBox.Size.Y);

            parentTable.AddTableRow(new MyGuiControlLabel(null, null, "Name"));
            parentTable.AddTableRow(m_nameBox);

            MyGuiControlButton spawnRingButton = MyPluginGuiHelper.CreateDebugButton(usableWidth, "Add ring", delegate
            {
                StringBuilder name = new StringBuilder();
                m_nameBox.GetText(name);
                if(name.Length < 4)
                {
                    MyPluginGuiHelper.DisplayError("Name must be at least 4 letters long", "Error");
                    return;
                }

                MySystemAsteroids instance;
                MySystemRing ring;
                GenerateAsteroidData(out ring, out instance);

                if(ring == null || instance == null)
                {
                    MyPluginGuiHelper.DisplayError("Could not generate asteroid ring. No data found.", "Error");
                    return;
                }

                MyAsteroidRingProvider.Static.AddInstance(instance, ring, delegate (bool success)
                {
                    if (!success)
                    {
                        MyPluginGuiHelper.DisplayError("Ring could not be added. An object with that name already exists.", "Error");
                    }
                    else
                    {
                        MyPluginGuiHelper.DisplayMessage("Ring was created successfully.", "Success");
                    }
                });
            });

            parentTable.AddTableRow(spawnRingButton);

            return true;
        }

        public void OnAdminMenuClose()
        {
            m_fetchedStarSytem = null;
            m_parentScreen = null;
            m_currentSelectedAsteroid = null;
            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);
        }

        /// <summary>
        /// Updates the visual representaion of the current ring edited in the spawn menu
        /// </summary>
        private void UpdateRingVisual()
        {
            MySystemRing ring = GenerateAsteroidRing();
            if (ring == null) return;

            var shape = MyAsteroidObjectShapeRing.CreateFromRingItem(ring);

            MyPluginDrawSession.Static.RemoveRenderObject(PREVIEW_RENDER_ID);

            MyPluginDrawSession.Static.AddRenderObject(PREVIEW_RENDER_ID, new RenderHollowCylinder(shape.worldMatrix, (float)shape.radius + (float)shape.width, (float)shape.radius, (float)shape.height, Color.LightGreen.ToVector4()));
        }

        private void OnRemoveRing(MyGuiControlButton button)
        {
            MyPluginLog.Debug("Removing ring " + m_currentSelectedAsteroid.DisplayName);

            MyStarSystemGenerator.Static.RemoveObjectFromSystem(m_currentSelectedAsteroid.DisplayName, delegate (bool success)
            {
                if (success)
                {
                    m_parentScreen.ForceFetchStarSystem = true;
                    m_parentScreen.ShouldRecreate = true;

                    MyPluginLog.Debug("Refreshing admin menu");
                }
                else
                {
                    MyPluginGuiHelper.DisplayError(m_currentSelectedAsteroid.DisplayName + " could not be deleted", "Error");
                }
            });
        }

        /// <summary>
        /// Teleports the player to the selected ring
        /// </summary>
        /// <param name="button">Button to call</param>
        private void OnTeleportToRing(MyGuiControlButton button)
        {
            MyPluginLog.Debug("Teleporting player to " + m_currentSelectedAsteroid.DisplayName);

            if (MySession.Static.CameraController != MySession.Static.LocalCharacter || true)
            {
                if (m_currentSelectedAsteroid != null)
                {
                    IMyAsteroidObjectShape shape = MyAsteroidRingProvider.Static.GetAsteroidObjectShape(m_currentSelectedAsteroid);
                    if(shape == null) 
                    {
                        MyPluginGuiHelper.DisplayError("Cant teleport to asteroid ring. It does not exist", "Error");
                        return;
                    }

                    m_parentScreen.CloseScreenNow();

                    MyMultiplayer.TeleportControlledEntity(shape.GetPointInShape());
                    MyGuiScreenGamePlay.SetCameraController();
                }
            }
        }

        /// <summary>
        /// Generates the whole data for the currently edited asteroid ring from the values in the spawn menu
        /// </summary>
        /// <param name="ringData">The out value for the ring data</param>
        /// <param name="systemObject">The out value for the system object</param>
        private void GenerateAsteroidData(out MySystemRing ringData, out MySystemAsteroids systemObject)
        {
            if (m_parentObjectListBox.SelectedItems.Count <= 0)
            {
                ringData = null;
                systemObject = null;
                return;
            }
            var selectedParent = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
            var parentItem = selectedParent.UserData as MySystemObject;
            StringBuilder name = new StringBuilder();
            m_nameBox.GetText(name);

            systemObject = new MySystemAsteroids();
            systemObject.AsteroidTypeName = MyAsteroidRingProvider.Static.GetTypeName();
            systemObject.CenterPosition = parentItem.CenterPosition;
            systemObject.AsteroidSize = new MySerializableMinMax((int)m_asteroidSizesSlider.CurrentMin, (int)m_asteroidSizesSlider.CurrentMax);
            systemObject.DisplayName = name.ToString();

            ringData = GenerateAsteroidRing();
        }

        /// <summary>
        /// Generates an asteroid ring from the current slider values in the spawn menu
        /// </summary>
        /// <returns>The generated system ring data</returns>
        private MySystemRing GenerateAsteroidRing()
        {
            if (m_parentObjectListBox.SelectedItems.Count <= 0) return null;
            var selected = m_parentObjectListBox.SelectedItems[m_parentObjectListBox.SelectedItems.Count - 1];
            MySystemObject parent = selected.UserData as MySystemObject;

            MySystemRing ring = new MySystemRing();
            ring.CenterPosition = parent.CenterPosition;
            ring.Width = m_widthSlider.Value * 1000;
            ring.Height = ring.Width / 10;
            ring.Radius = m_radiusSlider.Value * 1000;
            ring.AngleDegrees = new Vector3D(m_angleXSlider.Value, m_angleYSlider.Value, m_angleZSlider.Value);
            return ring;
        }

        /// <summary>
        /// Runs on click of the parent item box. Sets the ranges for the sliders and resets the values.
        /// </summary>
        /// <param name="box">The listbox which calls this method on item clicked</param>
        private void OnParentItemClicked(MyGuiControlListbox box)
        {
            if(box.SelectedItems.Count > 0)
            {
                var parent = box.SelectedItems[box.SelectedItems.Count - 1].UserData as MySystemObject;
                var settings = MySettingsSession.Static.Settings.GeneratorSettings;

                if(parent == m_fetchedStarSytem.CenterObject)
                {
                    m_radiusSlider.MinValue = settings.MinMaxOrbitDistance.Min / 1000;
                    m_radiusSlider.MaxValue = settings.WorldSize < 0 ? int.MaxValue / 1000 : settings.WorldSize / 1000;
                    m_radiusSlider.Value = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;
                    m_radiusSlider.Enabled = true;

                    m_widthSlider.MinValue = settings.MinMaxOrbitDistance.Min / 2000;
                    m_widthSlider.MaxValue = settings.MinMaxOrbitDistance.Max / 1000;
                    m_widthSlider.DefaultValue = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;
                    m_widthSlider.Enabled = true;

                    m_asteroidSizesSlider.Enabled = true;
                    m_asteroidSizesSlider.SetValues(32, 1024);

                    m_angleXSlider.Enabled = true;
                    m_angleXSlider.Value = 0;
                    m_angleYSlider.Enabled = true;
                    m_angleYSlider.Value = 0;
                    m_angleZSlider.Enabled = true;
                    m_angleZSlider.Value = 0;

                    CameraLookAt(Vector3D.Zero, new Vector3D(0, 0, m_radiusSlider.Value * 2000));
                    UpdateRingVisual();
                    return;
                }

                if (parent.Type != MySystemObjectType.PLANET && parent.Type != MySystemObjectType.MOON) return;
                var planet = parent as MySystemPlanet;

                m_radiusSlider.MinValue = (int)planet.Diameter / 1000 * 0.75f;
                m_radiusSlider.MaxValue = (int)planet.Diameter / 1000 * 2f;
                m_radiusSlider.DefaultValue = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;
                m_radiusSlider.Enabled = true;

                m_widthSlider.MinValue = (int)planet.Diameter / 1000 / 20f;
                m_widthSlider.MaxValue = (int)planet.Diameter / 1000 / 1.25f;
                m_widthSlider.DefaultValue = m_radiusSlider.MinValue + (m_radiusSlider.MaxValue - m_radiusSlider.MinValue) / 2;
                m_widthSlider.Enabled = true;

                m_asteroidSizesSlider.Enabled = true;
                m_asteroidSizesSlider.SetValues(32, 1024);

                m_angleXSlider.Enabled = true;
                m_angleXSlider.Value = 0;
                m_angleYSlider.Enabled = true;
                m_angleYSlider.Value = 0;
                m_angleZSlider.Enabled = true;
                m_angleZSlider.Value = 0;

                m_nameBox.SetText(new StringBuilder(""));

                CameraLookAt(planet.CenterPosition, (float)planet.Diameter * 2f);
                UpdateRingVisual();
            }
            else
            {
                m_radiusSlider.Enabled = false;
                m_widthSlider.Enabled = false;
                m_asteroidSizesSlider.Enabled = false;
                m_angleXSlider.Enabled = false;
                m_angleYSlider.Enabled = false;
                m_angleZSlider.Enabled = false;
            }
        }

        /// <summary>
        /// Makes the spectator cam look at the specific point from the given distance
        /// </summary>
        /// <param name="center">Point to look at</param>
        /// <param name="distance">Distance to look from</param>
        private void CameraLookAt(Vector3D center, float distance)
        {
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            MySpectatorCameraController.Static.Position = center + distance;
            MySpectatorCameraController.Static.Target = center;
        }

        /// <summary>
        /// Makes the spectator cam look at the specific point from the given distance
        /// </summary>
        /// <param name="center">Point to look at</param>
        /// <param name="distance">Distance to look from</param>
        private void CameraLookAt(Vector3D center, Vector3D distance)
        {
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            MySpectatorCameraController.Static.Position = center + distance;
            MySpectatorCameraController.Static.Target = center;
        }
    }
}
