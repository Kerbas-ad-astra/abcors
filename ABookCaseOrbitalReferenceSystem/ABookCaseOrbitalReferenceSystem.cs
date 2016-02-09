﻿using System;
using UnityEngine;
using KSP.IO;
using System.Linq;
using System.Globalization;

namespace ABCORS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class ABookCaseOrbitalReferenceSystem : MonoBehaviour
    {
        private bool _mouseOver = false;
        private PatchedConics.PatchCastHit _mousedOverPatch;

        private Rect _popup = new Rect(0f, 0f, 160f, 88f);

        private void Awake()
        {
            _popup.center = new Vector2(Screen.width * 0.5f - _popup.width * 0.5f,
                Screen.height * 0.5f - _popup.height * 0.5f);
        }

        private void Update()
        {
            _mouseOver = MouseOverOrbit(out _mousedOverPatch);

            if (!_mouseOver)
                return;

            var updatedLocation = _mousedOverPatch.GetUpdatedScreenPoint();

            _popup.center = new Vector2(updatedLocation.x, Screen.height - updatedLocation.y);
        }

        private void OnGUI()
        {
            if (!_mouseOver)
                return;

            GUI.skin = HighLogic.Skin;

            Orbit orbit = _mousedOverPatch.pr.patch;
            Vector3d deltaPos = orbit.getPositionAtUT(_mousedOverPatch.UTatTA) - orbit.referenceBody.position;
            double altitude = deltaPos.magnitude - orbit.referenceBody.Radius;

            GUILayout.BeginArea(GUIUtility.ScreenToGUIRect(_popup));
            GUILayout.Label("T: " + KSPUtil.PrintTime((int)(Planetarium.GetUniversalTime() - _mousedOverPatch.UTatTA), 5, true) + "\nAlt: " + altitude.ToString("N0", CultureInfo.CurrentCulture) + "m");

            GUILayout.EndArea();
        }

        private bool MouseOverOrbit(out PatchedConics.PatchCastHit hit)
        {
            hit = default(PatchedConics.PatchCastHit);

            if (FlightGlobals.ActiveVessel == null)
                return false;

            var patchRenderer = FlightGlobals.ActiveVessel.patchedConicRenderer;

            if (patchRenderer == null)
                return false;

            var patches = patchRenderer.solver.maneuverNodes.Any()
                ? patchRenderer.flightPlanRenders
                : patchRenderer.patchRenders;

            return PatchedConics.ScreenCast(Input.mousePosition, patches, out hit);
        }
    }
}
