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
            if (!_fragmentTypeDictionary.ContainsKey(FragmentType.BspTree)
            || !_fragmentTypeDictionary.ContainsKey(FragmentType.BspRegion)
            || !_fragmentTypeDictionary.ContainsKey(FragmentType.BspRegionType))
            {
                return;
            }
            
            BspTree tree = _fragmentTypeDictionary[FragmentType.BspTree][0] as BspTree;

            if (tree == null)
            {
                return;
            }
            
            tree.LinkBspRegions(_bspRegions);
            
            foreach (WldFragment fragment in _fragmentTypeDictionary[FragmentType.BspRegionType])
            {
                BspRegionType bspRegion = fragment as BspRegionType;

                bspRegion?.LinkRegionType(_bspRegions);
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
            if (!_fragmentTypeDictionary.ContainsKey(FragmentType.AmbientLightColor))
            {
                _logger.LogWarning("Cannot export ambient light color. No fragment found.");
                return;
            }
            
            AmbientLightColorWriter writer = new AmbientLightColorWriter();

            writer.AddFragmentData(_fragmentTypeDictionary[FragmentType.AmbientLightColor][0] as AmbientLightColor);
            writer.WriteAssetToFile(_zoneName + "/ambient_light.txt");        
        }

        private void ExportBspTree()
        {
            if (!_fragmentTypeDictionary.ContainsKey(FragmentType.BspTree))
            {
                _logger.LogWarning("Cannot export BSP tree. No tree found.");
                return;
            }
            BspTreeWriter writer = new BspTreeWriter();

            writer.AddFragmentData(_fragmentTypeDictionary[FragmentType.BspTree][0] as BspTree);
            writer.WriteAssetToFile(_zoneName + "/bsp_tree.txt");
        }
    }
}