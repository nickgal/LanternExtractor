﻿using LanternExtractor.EQ.Pfs;
using LanternExtractor.EQ.Wld.Exporters;
using LanternExtractor.EQ.Wld.Fragments;
using LanternExtractor.Infrastructure.Logger;

namespace LanternExtractor.EQ.Wld
{
    public class WldFileZone : WldFile
    {
        public WldFileZone(PfsFile wldFile, string zoneName, WldType type, ILogger logger, Settings settings,
            WldFile wldToInject = null) : base(wldFile, zoneName, type, logger, settings, wldToInject)
        {
        }

        protected override void ProcessData()
        {
            base.ProcessData();
            LinkBspReferences();
        }

        private void LinkBspReferences()
        {
            var bspTree = GetFragmentsOfType2<BspTree>();

            if (bspTree.Count == 0)
            {
                return;
            }

            var bspRegions = GetFragmentsOfType2<BspRegion>();

            if (bspRegions.Count == 0)
            {
                return;
            }
            
            bspTree[0].LinkBspRegions(bspRegions);

            var regionTypes = GetFragmentsOfType2<BspRegionType>();
            
            foreach (var regionType in regionTypes)
            {
                regionType.LinkRegionType(bspRegions);
            }
        }
        
        protected override void ExportData()
        {
            base.ExportData();
            ExportAmbientLightColor();
            ExportBspTree();

            if (_settings.ExportZoneMeshGroups)
            {
                ExportMeshList();
            }
        }

        private void ExportAmbientLightColor()
        {
            var ambientLight = GetFragmentsOfType2<GlobalAmbientLight>();
            
            if (ambientLight.Count == 0)
            {
                return;
            }
            
            AmbientLightColorWriter writer = new AmbientLightColorWriter();
            writer.AddFragmentData(ambientLight[0]);
            writer.WriteAssetToFile(_zoneName + "/ambient_light.txt");        
        }

        private void ExportBspTree()
        {
            var bspTree = GetFragmentsOfType2<BspTree>();
            
            if (bspTree.Count == 0)
            {
                return;
            }
            
            BspTreeWriter writer = new BspTreeWriter();
            writer.AddFragmentData(bspTree[0]);
            writer.WriteAssetToFile(_zoneName + "/bsp_tree.txt");
        }
    }
}