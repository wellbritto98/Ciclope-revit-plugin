using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RevitTemplate.Utils;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Service to add shared parameters to all families in the project.
    /// </summary>
    public class FamilyParameterService
    {
        private readonly Document _doc;
        private readonly Application _app;
        private readonly string _sharedParameterFilePath;

        public FamilyParameterService(Document doc, Application app, string sharedParameterFilePath)
        {
            _doc = doc;
            _app = app;
            _sharedParameterFilePath = sharedParameterFilePath;
        }

        public void AddCiclopeParametersToAllFamilies()
        {
            // 1. Garante que o arquivo de parâmetros compartilhados existe
            if (string.IsNullOrWhiteSpace(_sharedParameterFilePath))
                throw new FileNotFoundException("O caminho do arquivo de parâmetros compartilhados está vazio.");

            if (!File.Exists(_sharedParameterFilePath))
            {
                // Gera GUIDs únicos para cada parâmetro
                var guidBase = Guid.NewGuid();
                var guidEstado = Guid.NewGuid();
                var guidCodigo = Guid.NewGuid(); var content =
                    "# This is a Revit shared parameter file.\r\n" +
                    "# Do not edit manually.\r\n" +
                    "*META\tVERSION\tMINVERSION\r\n" +
                    "META\t2\t1\r\n" +
                    "*GROUP\tID\tNAME\r\n" +
                    "GROUP\t1\tCICLOPE\r\n" +
                    "*PARAM\tGUID\tNAME\tDATATYPE\tDATACATEGORY\tGROUP\tVISIBLE\tDESCRIPTION\tUSERMODIFIABLE\tHIDEWHENNOVALUE\r\n" +
                    $"PARAM\t{guidBase}\tBase\tTEXT\t\t1\t1\tBase para integração CICLOPE\t0\t0\r\n" +
                    $"PARAM\t{guidEstado}\tEstado\tTEXT\t\t1\t1\tEstado para integração CICLOPE\t0\t0\r\n" +
                    $"PARAM\t{guidCodigo}\tCodigo\tTEXT\t\t1\t1\tCodigo para integração CICLOPE\t0\t0\r\n";
                var encoding = System.Text.Encoding.GetEncoding(1252); // ANSI
                File.WriteAllText(_sharedParameterFilePath, content, encoding);
            }

            // 2. Seta o caminho no Revit
            _app.SharedParametersFilename = _sharedParameterFilePath;

            // 3. Abre o arquivo de parâmetros compartilhados
            DefinitionFile defFile = _app.OpenSharedParameterFile();
            if (defFile == null)
                throw new InvalidOperationException(
                    $"Falha ao abrir o arquivo de parâmetros compartilhados: '{_sharedParameterFilePath}'. " +
                    "O arquivo pode estar corrompido, inacessível ou em formato inválido.");

            // 4. Cria grupo se não existir
            string grupoNome = "CICLOPE";
            DefinitionGroup group = defFile.Groups.get_Item(grupoNome) ?? defFile.Groups.Create(grupoNome);

            // 5. Cria definições se não existirem
            var parametros = new[] { "Base", "Estado", "Codigo" };
            var definitions = new List<Definition>();
            foreach (var param in parametros)
            {
                Definition def = group.Definitions.get_Item(param);
                if (def == null)
                {
                    var options = new ExternalDefinitionCreationOptions(param, SpecTypeId.String.Text)
                    {
                        Visible = true
                    };
                    def = group.Definitions.Create(options);
                }
                definitions.Add(def);
            }

            // 6. Coleta todas as famílias carregadas no projeto
            FilteredElementCollector famCollector = new FilteredElementCollector(_doc)
                .OfClass(typeof(Family));

            HashSet<ElementId> categoriaIds = new HashSet<ElementId>();
            foreach (Family family in famCollector)
            {
                Category cat = family.FamilyCategory;
                if (cat != null)
                    categoriaIds.Add(cat.Id);
            }

            if (categoriaIds.Count == 0)
            {
                throw new InvalidOperationException("Nenhuma categoria de família encontrada no projeto.");
            }

            // 7. Cria CategorySet dinâmico
            CategorySet categorySet = _app.Create.NewCategorySet();
            foreach (var catId in categoriaIds)
            {
                Category cat = Category.GetCategory(_doc, catId);
                if (cat != null && cat.AllowsBoundParameters && cat.CategoryType == CategoryType.Model)
                    categorySet.Insert(cat);
            }

            // 8. Adiciona os parâmetros às categorias
            using (Transaction t = new Transaction(_doc, "Adicionar parâmetros CICLOPE às famílias"))
            {
                t.Start();
                BindingMap map = _doc.ParameterBindings;
                foreach (var def in definitions)
                {
                    var binding = _app.Create.NewInstanceBinding(categorySet);
                    map.Insert(def, binding, BuiltInParameterGroup.PG_DATA);
                }
                t.Commit();
            }
        }

        public void AddCiclopeParametersToSpecificElements(List<int> elementIds)
        {
            if (elementIds == null || elementIds.Count == 0)
                throw new ArgumentException("A lista de IDs de elementos está vazia ou nula.");

            // 1. Garante que o arquivo de parâmetros compartilhados existe
            if (string.IsNullOrWhiteSpace(_sharedParameterFilePath) || !File.Exists(_sharedParameterFilePath))
                throw new FileNotFoundException($"O arquivo de parâmetros compartilhados não existe: '{_sharedParameterFilePath}'");

            // 2. Seta o caminho no Revit
            _app.SharedParametersFilename = _sharedParameterFilePath;

            // 3. Abre o arquivo de parâmetros compartilhados
            DefinitionFile defFile = _app.OpenSharedParameterFile();
            if (defFile == null)
                throw new InvalidOperationException(
                    $"Falha ao abrir o arquivo de parâmetros compartilhados: '{_sharedParameterFilePath}'.");

            // 4. Cria grupo se não existir
            string grupoNome = "CICLOPE";
            DefinitionGroup group = defFile.Groups.get_Item(grupoNome) ?? defFile.Groups.Create(grupoNome);

            // 5. Cria definições se não existirem
            var parametros = new[] { "Base", "Estado", "Codigo" };
            var definitions = new List<Definition>();
            foreach (var param in parametros)
            {
                Definition def = group.Definitions.get_Item(param);
                if (def == null)
                {
                    var options = new ExternalDefinitionCreationOptions(param, SpecTypeId.String.Text)
                    {
                        Visible = true
                    };
                    def = group.Definitions.Create(options);
                }
                definitions.Add(def);
            }

            // 6. Coleta apenas os elementos específicos por ID
            HashSet<ElementId> specificElementIds = new HashSet<ElementId>();
            HashSet<ElementId> categoriaIds = new HashSet<ElementId>();

            foreach (int id in elementIds)
            {
                ElementId elementId = new ElementId(id);
                Element element = _doc.GetElement(elementId);

                if (element != null)
                {
                    specificElementIds.Add(elementId);
                    if (element.Category != null)
                    {
                        categoriaIds.Add(element.Category.Id);
                    }
                }
            }

            if (categoriaIds.Count == 0)
            {
                throw new InvalidOperationException("Nenhuma categoria válida encontrada nos elementos selecionados.");
            }

            // 7. Cria CategorySet para as categorias encontradas
            CategorySet categorySet = _app.Create.NewCategorySet();
            foreach (var catId in categoriaIds)
            {
                Category cat = Category.GetCategory(_doc, catId);
                if (cat != null && cat.AllowsBoundParameters && cat.CategoryType == CategoryType.Model)
                    categorySet.Insert(cat);
            }

            // 8. Adiciona os parâmetros às categorias
            using (Transaction t = new Transaction(_doc, "Adicionar parâmetros CICLOPE aos elementos selecionados"))
            {
                t.Start();
                BindingMap map = _doc.ParameterBindings;
                foreach (var def in definitions)
                {
                    var binding = _app.Create.NewInstanceBinding(categorySet);
                    map.Insert(def, binding, BuiltInParameterGroup.PG_DATA);
                }
                t.Commit();
            }
        }

    }
    public class FamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            source = default;
            return true;
        }
    }
}
