﻿using LanternExtractor.EQ.Pfs;
using LanternExtractor.EQ.Wld.DataTypes;
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

            tree?.LinkBspRegions(_fragments);
            
            foreach (WldFragment fragment in _fragmentTypeDictionary[FragmentType.BspRegionType])
            {
                BspRegionType bspRegion = fragment as BspRegionType;

                bspRegion?.LinkRegionType(_bspRegions);
            }
        }
        
        protected override void ExportData()
        {
            base.ExportData();
            ExportZoneMeshes();
            ExportBspTree();
        }

        private void ExportZoneMeshes()
        {
            if (!_fragmentTypeDictionary.ContainsKey(FragmentType.Mesh))
            {
                _logger.LogWarning("Cannot export zone meshes. No meshes found.");
                return;
            }
            
            string zoneExportFolder = _zoneName + "/" + LanternStrings.ExportZoneFolder;

            MeshObjExporter meshExporter = new MeshObjExporter(ObjExportType.Textured, _settings.ExportHiddenGeometry, _settings.ExportZoneMeshGroups, _zoneName);
            MeshObjExporter collisionMeshExport = new MeshObjExporter(ObjExportType.Collision, _settings.ExportHiddenGeometry, _settings.ExportZoneMeshGroups, _zoneName);
            
            foreach (WldFragment listFragment in _fragmentTypeDictionary[FragmentType.Mesh])
            {
                meshExporter.AddFragmentData(listFragment);
                collisionMeshExport.AddFragmentData(listFragment);
            }
            
            meshExporter.WriteAssetToFile(zoneExportFolder + _zoneName + LanternStrings.ObjFormatExtension);
            collisionMeshExport.WriteAssetToFile(zoneExportFolder + _zoneName +  "_collision" + LanternStrings.ObjFormatExtension);

            MeshObjMtlExporter mtlExporter = new MeshObjMtlExporter(_settings, _zoneName);
            
            foreach (WldFragment listFragment in _fragmentTypeDictionary[FragmentType.MaterialList])
            {
                mtlExporter.AddFragmentData(listFragment);
            }
            
            mtlExporter.WriteAssetToFile(zoneExportFolder + _zoneName + LanternStrings.FormatMtlExtension);
        }
        
        private void ExportBspTree()
        {
            if (!_fragmentTypeDictionary.ContainsKey(FragmentType.BspTree))
            {
                _logger.LogWarning("Cannot export BSP tree. No tree found.");
                return;
            }
            BspTreeExporter exporter = new BspTreeExporter();

            exporter.AddFragmentData(_fragmentTypeDictionary[FragmentType.BspTree][0] as BspTree);
            
            exporter.WriteAssetToFile(_zoneName + "/" + _zoneName + "_bsp_tree.txt");
        }
    }
}