using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cerebello.Firestarter.Helpers
{
    internal class Cbhpm
    {
        public static Cbhpm LoadData(string pathOfTxt)
        {
            var result = new Cbhpm();
            result.LoadFromTextCopiedOfPdf(File.ReadAllText(pathOfTxt, Encoding.GetEncoding(1252)));
            return result;
        }

        public Cbhpm()
        {
            this.Capitulos = new Dictionary<int, Capitulo>();
            this.Paginas = new List<Pagina>();
            this.Items = new Dictionary<StructTuple<string, int>, object>();
        }

        /// <summary>
        /// Date that is used as the version of the document.
        /// </summary>
        public string DataVersao { get; set; }

        public Dictionary<int, Capitulo> Capitulos { get; private set; }
        public List<Pagina> Paginas { get; private set; }
        public Dictionary<StructTuple<string, int>, object> Items { get; private set; }

        public void LoadFromTextCopiedOfPdf(string text)
        {
            // Reading all codes that exist in the text, so that we can compare with
            // the list of processed codes to know if any is missing at the end.
            var codigosExistentes = new HashSet<string>();
            var matchCodigos = Regex.Matches(text, @"\d\s?\.\s?\d\d\s?\.\s?\d\d\s?\.\s?\d\d\s?\-\s?\d\d?");
            foreach (Match eachMatch in matchCodigos)
                codigosExistentes.Add(eachMatch.Value);
            var codigosUsados = new HashSet<string>();

            // Regexes that are going to be used.
            var regexCapitulo = new Regex(@"^\s*(?<NUM>\d+)?\s*CAPÍTULO\s*(?<NUM>\d+)?\s*(?<NAME>.*?)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var regexProcedimentoTipo = new Regex(
                @"^([\s\w/\-]+?)\s+(\d\.\d\d\.\d\d\.\d\d\-\d)$",
                RegexOptions.IgnoreCase);

            var regexProcedimentoSubtipo = new Regex(
                @"^(.*?)\s+\(\s?(\d\.\d\d\.\d\d\.\d\d\-\d)\)$",
                RegexOptions.IgnoreCase);

            Regex regexProcedimentosCapitulo = null; // this one is dynamic

            var regexColumnTitles = new Regex(
                @"^(?:(?:(Código|Procedimentos|Porte|Custo|Oper\.|Nº de|Aux\.?|Anest\.|Inc\.|Filme|ou Doc|UR)|(\w+\.?))(?:\s+|$))+$",
                RegexOptions.IgnoreCase);

            var mapColNamePattern = new Dictionary<string, string>
            {
                { "código", null },
                { "procedimentos", null },
                { "porte", @"(?<PORTE>(?:(?:[\d,]+\s*de\s*)?\d+[a-z]+)|-)" },
                { "custo oper", @"(?<CUSTO>-|\d+,\d{2,3})" },
                { "porte anest", @"(?<ANEST>-|\d+)" },
                { "nº de aux", @"(?<AUX>-|\d+)" },
                { "ur", @"(?<UR>\*)" },
                { "filme ou doc", @"(?<DOC>-|\d+,\d{4})" },
                { "inc", @"(?<INC>-|\d+)" },
            };

            const string patternProcLine = @"
                ^
                (?<CODE>\d\.\d\d\.\d\d\.\d\d\-\d)?\s*
                (?<NAME>.+?)\s*
                (?:
                  (?<DOTS>[\.\s]*\.\s*)
                  {0}
                )?
                \s*
                (?:(?=\r\n|\r|\n|$))
                ";

            Regex regexProcLine = null;

            var regexAnexo = new Regex(@"^(OBSERVAÇÕES.*?|OBSERVAÇão.*?|INSTRUÇÕES.*?)\s*\:?$", RegexOptions.IgnoreCase);

            var regexPageFooter = new Regex(
                @"^(?<PAGE>\d+)?\s*Classificação Brasileira Hierarquizada de Procedimentos Médicos - (?:\d{4}|(?:\d{1,2}.*?edi[cç][aã]o))\s*(?<PAGE>\d+)?$",
                RegexOptions.IgnoreCase);

            var regexNumber = new Regex(@"^\d+$");

            // State variables.
            Pagina curPagina = null;
            Capitulo curCapitulo = null;
            ProcTipo curProcTipo = null;
            ProcSubtipo curProcSubtipo = null;
            TextoAnexo curAnexo = null; // this can span multiples pages
            var builder = new StringBuilder(10000);
            int pageNum = 0;
            string curGroupName = null; // this happens at page 154 e 155
            bool isExplicitAttachment = false; // this happens at page 202

            // Processing all pages and lines of the text.
            var splitPages = text.Split('\f');
            for (int itPage = 0; itPage < splitPages.Length; itPage++)
            {
                var pageText = splitPages[itPage];

                var matchExplicitMissing = Regex.Matches(pageText, @"####EXPLICIT-MISSING:(.*?)(?=####|\r\n|\n|\r|$)");
                foreach (var eachMatch in matchExplicitMissing.Cast<Match>())
                    codigosUsados.Add(eachMatch.Groups[1].Value);

                pageText = Regex.Replace(pageText, @" *####(?:ERRO|EXPLICIT-MISSING).*?(?=\r|\n|$)", "");

                curPagina = this.CreatePagina(itPage, Regex.Replace(pageText, @" *####.*?(?=\r|\n|$)", ""));

                var matchCapitulo = regexCapitulo.Match(pageText);

                if (matchCapitulo.Success)
                {
                    // If there is an attachment being read, we need to finalize it.
                    if (curAnexo != null)
                    {
                        curAnexo.Texto = builder.ToString().Trim();
                        CheckForMultipleLines(curAnexo.Texto);
                        builder.Clear();
                        curAnexo = null;
                    }

                    // If this page is the chapter title we must create a new chapter object.
                    int num = int.Parse(matchCapitulo.Groups["NUM"].Value);
                    string name = Regex.Replace(matchCapitulo.Groups["NAME"].Value, @"\s+", " ");

                    curCapitulo = this.CreateCapitulo(num, name, curPagina);

                    // Setting the new value of the regex, that depends on the name of the chapter.
                    regexProcedimentosCapitulo = new Regex(
                        string.Format(@"^{0}$", Regex.Replace(name, @"\s+", @"\s+")),
                        RegexOptions.IgnoreCase);

                    continue;
                }

                if (curCapitulo != null)
                {
                    // State variables to read the lines.
                    bool canReadCapitulo = true;
                    bool canReadProcTipo = true;
                    bool canReadColumnTitle = true;
                    int columnTitleLines = 0; // lines of text devoted to the column titles.

                    bool foundPageFooter = false;
                    bool foundPageNum = false;

                    var splitLines = Regex.Split(pageText, @"\r\n|\r|\n");
                    for (int itLine = 0; itLine < splitLines.Length; itLine++)
                    {
                        var lineText = splitLines[itLine].Trim();

                        if (lineText.Contains("####ANEXO-INICIO"))
                        {
                            isExplicitAttachment = true;
                        }

                        if (lineText.Contains("####ANEXO-FIM"))
                        {
                            isExplicitAttachment = false;
                            lineText = Regex.Replace(lineText, @" *####.*?(?=\r|\n|$)", "");
                        }

                        if (string.IsNullOrEmpty(lineText))
                        {
                            // We use blank lines only in the attachments, such as observations and instructions.
                            bool canReadAnexo = !canReadCapitulo && !canReadProcTipo && !canReadColumnTitle && !foundPageFooter;
                            if (curAnexo != null && canReadAnexo)
                            {
                                builder.AppendLine();
                            }

                            continue;
                        }

                        if (lineText.Contains("####GROUP"))
                        {
                            lineText = Regex.Replace(lineText, @" *####.*?(?=\r|\n|$)", "");
                            curGroupName = lineText;

                            continue;
                        }

                        if (lineText.Contains("####ANEXO-CAPITULO"))
                        {
                            // This item has no specific code.
                            lineText = Regex.Replace(lineText, @" *####.*?(?=\r|\n|$)", "");
                            curAnexo = this.CreateAnexo(
                                null,
                                lineText.Trim(':').Trim().ToUpperInvariant(),
                                curPagina,
                                null,
                                null);

                            curAnexo.Capitulo = curCapitulo;
                            curCapitulo.Anexos.Add(curAnexo);

                            builder.Clear();
                            builder.AppendLine(lineText);

                            continue;
                        }

                        // Chapter heading that exists in every page.
                        // When we find this heading, then we set the page's chapter property.
                        if (canReadCapitulo)
                        {
                            var matchProcCapitulo = regexProcedimentosCapitulo.Match(lineText);

                            if (matchProcCapitulo.Success)
                            {
                                // Seting the chapter that the page belongs to.
                                curPagina.Capitulo = curCapitulo;

                                canReadCapitulo = false;

                                continue;
                            }
                        }

                        // Reading the ProcTipo heading that exists after the Chapter heading.
                        // This heading contains the level-2 node of the CBHPM tree.
                        if (canReadProcTipo)
                        {
                            var matchProcedimentoTipo = regexProcedimentoTipo.Match(lineText);

                            if (matchProcedimentoTipo.Success)
                            {
                                // All pages inside the same ProcTipo have the same heading,
                                // so we use a method that creates the object if it does not exist,
                                // or returns the existing one.
                                var curProcTipo2 = this.GetOrCreateProcTipo(
                                    matchProcedimentoTipo.Groups[2].Value,
                                    matchProcedimentoTipo.Groups[1].Value,
                                    curPagina,
                                    curCapitulo);

                                if (curProcTipo != curProcTipo2)
                                {
                                    // If there is an attachment being read, we need to finalize it.
                                    if (curAnexo != null)
                                    {
                                        curAnexo.Texto = builder.ToString().Trim();
                                        CheckForMultipleLines(curAnexo.Texto);
                                        builder.Clear();
                                        curAnexo = null;
                                    }
                                }

                                curProcTipo = curProcTipo2;

                                codigosUsados.Add(curProcTipo.Codigo);

                                canReadProcTipo = false;

                                continue;
                            }
                        }

                        // All pages that contains medical procedures, have the titles of the columns.
                        // We need these columns to know what is inside each column, and set the values
                        // correctly.
                        if (canReadColumnTitle)
                        {
                            var matchColumnTitles = regexColumnTitles.Match(lineText);

                            if (matchColumnTitles.Success && !matchColumnTitles.Groups[2].Success)
                            {
                                var columnCaptures = matchColumnTitles.Groups[1].Captures.Cast<Capture>().ToArray();
                                foreach (var eachCapture in columnCaptures)
                                    curPagina.ColunaAdd(eachCapture.Value.TrimEnd('.'));

                                columnTitleLines++;

                                if (columnTitleLines > 2)
                                    throw new Exception("More than 2 lines for column titles.");

                                continue;
                            }

                            if (columnTitleLines == 0)
                                throw new Exception("No column titles have been found.");

                            // If there is no more columns to read, then we create the regex to read
                            // each medical procedure.
                            var valuePatternsOfPage = new List<string>();
                            foreach (var eachColName in curPagina.Colunas)
                            {
                                var colPattern = mapColNamePattern[eachColName];
                                if (colPattern != null)
                                    valuePatternsOfPage.Add(colPattern);
                            }
                            var patternValues = string.Format(@"(?:{0})", string.Join(@"\s+", valuePatternsOfPage));
                            regexProcLine = new Regex(
                                string.Format(patternProcLine, patternValues),
                                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                            canReadColumnTitle = false;
                        }

                        // Reading the footer of the page.
                        // The footer can not come before the end of the page.
                        {
                            var matchPageFooter = regexPageFooter.Match(lineText);

                            if (matchPageFooter.Success)
                            {
                                if (foundPageFooter)
                                    throw new Exception("Page footer duplicated.");

                                var strPageNum = matchPageFooter.Groups["PAGE"].Value;
                                if (int.TryParse(strPageNum, out pageNum))
                                {
                                    if (curPagina.Numero != null && curPagina.Numero != pageNum)
                                        throw new Exception("Page already has a number.");

                                    if (foundPageNum)
                                        throw new Exception("Page number duplicated.");

                                    foundPageNum = true;

                                    curPagina.Numero = pageNum;
                                    pageNum = 0;
                                }

                                foundPageFooter = true;

                                continue;
                            }

                            var matchNumber = regexNumber.Match(lineText);

                            if (matchNumber.Success)
                            {
                                pageNum = int.Parse(lineText);

                                if (!foundPageFooter || curPagina.Numero != null && curPagina.Numero != pageNum)
                                    throw new Exception("Found a lost number.");

                                if (foundPageNum)
                                    throw new Exception("Page number duplicated.");

                                foundPageNum = true;

                                curPagina.Numero = pageNum;
                                pageNum = 0;

                                continue;
                            }
                        }

                        if (foundPageFooter)
                            throw new Exception("Cannot read after page footer.");

                        // Reading the remaining of the attachment, if there is a current attachment.
                        // The text of the attachment always start by multiple spaces.
                        if (curAnexo != null)
                        {
                            if (splitLines[itLine].StartsWith("    ") || isExplicitAttachment)
                            {
                                lineText = Regex.Replace(splitLines[itLine], @" *####.*?(?=\r|\n|$)", "");
                                builder.AppendLine(lineText);
                                continue;
                            }
                        }

                        // If there is an attachment being read, we need to finalize it.
                        if (curAnexo != null)
                        {
                            curAnexo.Texto = builder.ToString().Trim();
                            CheckForMultipleLines(curAnexo.Texto);
                            builder.Clear();
                            curAnexo = null;
                        }

                        // Reading ProcSubtipo. A page may contain multiple subtipos.
                        // Each subtipo exists only once in the document, it is not repeated in all
                        // pages like ProcTipo.
                        {
                            var subtipoText = lineText;
                            Match matchProcedimentoSubtipo = null;
                            for (int itNext = 0; itNext < 3; itNext++)
                            {
                                matchProcedimentoSubtipo = regexProcedimentoSubtipo.Match(subtipoText);
                                if (matchProcedimentoSubtipo.Success)
                                {
                                    itLine += itNext;
                                    break;
                                }

                                var nextLine = splitLines[itLine + itNext + 1];
                                if (!Regex.IsMatch(nextLine, @"^\s?\w+") || Regex.IsMatch(nextLine, @"(?<!\()\d\.\d\d\.\d\d\.\d\d-\d(?!\))"))
                                    break;

                                // If not match, then we try to append the next line, and do the match again.
                                // Some SubtipoTexts are split in 2/3 lines.
                                subtipoText = string.Format("{0} {1}", subtipoText, nextLine);
                            }

                            if (matchProcedimentoSubtipo.Success)
                            {
                                curProcSubtipo = this.CreateProcSubtipo(
                                    matchProcedimentoSubtipo.Groups[2].Value,
                                    matchProcedimentoSubtipo.Groups[1].Value,
                                    curPagina,
                                    curProcTipo);

                                curGroupName = null;

                                codigosUsados.Add(curProcSubtipo.Codigo);

                                continue;
                            }


                        }

                        // Reading the medical procedures in the current page.
                        // Note that some of these items may not be procedures but observations.
                        {
                            var matchProcedimento = regexProcLine.Match(lineText);

                            if (matchProcedimento.Success && matchProcedimento.Groups["CODE"].Success)
                            {
                                string code = matchProcedimento.Groups["CODE"].Value;

                                var nameLine1 = matchProcedimento.Groups["NAME"].Value;

                                if (regexAnexo.IsMatch(nameLine1))
                                {
                                    curAnexo = this.CreateAnexo(
                                        code,
                                        nameLine1.Trim(':').Trim().ToUpperInvariant(),
                                        curPagina,
                                        curProcTipo,
                                        curProcSubtipo);

                                    codigosUsados.Add(code);

                                    builder.Clear();
                                    builder.AppendLine(nameLine1);
                                }
                                else
                                {
                                    builder.Clear();
                                    builder.Append(nameLine1);
                                    // Reading next lines until we find the end of the medical procedure.
                                    for (; itLine < splitLines.Length; )
                                    {
                                        if (matchProcedimento.Groups["PORTE"].Success)
                                        {
                                            string procName = builder.ToString();
                                            builder.Clear();

                                            string procPorte = matchProcedimento.Groups["PORTE"].Value;
                                            string procCusto = matchProcedimento.Groups["CUSTO"].Value;
                                            string procAnest = matchProcedimento.Groups["ANEST"].Value;
                                            string procAux = matchProcedimento.Groups["AUX"].Value;
                                            string procUR = matchProcedimento.Groups["UR"].Value;
                                            string procInc = matchProcedimento.Groups["INC"].Value;
                                            string procDoc = matchProcedimento.Groups["DOC"].Value;

                                            var proc = new Proc();
                                            proc.Cbhpm = this;
                                            proc.Codigo = code;
                                            proc.Nome = procName;
                                            proc.Porte = procPorte;
                                            proc.CustoOper = procCusto;
                                            proc.PorteAnest = procAnest;
                                            proc.NumAux = procAux;
                                            proc.Ur = procUR;
                                            proc.Inc = procInc;
                                            proc.FilmeOuDoc = procDoc;

                                            proc.GrupoNoSubtipo = curGroupName;

                                            proc.Subtipo = curProcSubtipo;
                                            curProcSubtipo.Procedimentos.Add(proc);

                                            proc.PaginaDeclarada = curPagina;

                                            var simpleCode = GetCodeSimple(code);
                                            this.Items.Add(StructTuple.Create(simpleCode, 0), proc);

                                            codigosUsados.Add(code);

                                            break;
                                        }

                                        itLine++;
                                        lineText = splitLines[itLine].Trim();

                                        matchProcedimento = regexProcLine.Match(lineText);

                                        if (Regex.IsMatch(lineText, @"\d\.\d\d\.\d\d\.\d\d-\d"))
                                            throw new Exception("Code must be found only in first line of medical procedure.");

                                        if (!splitLines[itLine].StartsWith("    "))
                                            throw new Exception("All lines after the first one of medical procedure, must start with spaces.");

                                        builder.Append(' ' + matchProcedimento.Groups["NAME"].Value);
                                    }

                                    if (itLine >= splitLines.Length)
                                        throw new Exception("Medical procedure not terminated.");
                                }

                                continue;
                            }

                            throw new Exception("Unknown line found.");
                        }
                    }
                }

                // Checking the columns of the page.
                FinalizarPagina(curPagina);

                // Finishing the page and checking page properties.
                if (curPagina.Capitulo != curCapitulo && !string.IsNullOrEmpty(pageText))
                    throw new Exception();

                if (curPagina.ProcTipo != curProcTipo && !string.IsNullOrEmpty(pageText))
                    throw new Exception();
            }

            // Checking codes that were not processed.
            var codigosNaoUsados = new HashSet<string>(codigosExistentes);
            codigosNaoUsados.ExceptWith(codigosUsados);

            if (codigosNaoUsados.Any())
                throw new Exception("There are unused codes!");
        }

        private static void CheckForMultipleLines(string str)
        {
            bool hasMultipleLines = Regex.IsMatch(str, @"(?:\r\n|\n){4}");

            if (hasMultipleLines)
                throw new Exception("Attachment with multiple line breaks.");
        }

        private TextoAnexo CreateAnexo(string code, string name, Pagina curPagina, ProcTipo curProcTipo, ProcSubtipo curProcSubtipo)
        {
            var result = new TextoAnexo
            {
                Cbhpm = this,
                Codigo = code,
                Nome = name,
                PaginaDeclarada = curPagina,
            };

            if (!string.IsNullOrEmpty(code))
            {
                if (code.Contains(".99-"))
                {
                    result.Subtipo = curProcSubtipo;
                    curProcSubtipo.Anexos.Add(result);
                }
                else if (code.Contains(".99."))
                {
                    result.Tipo = curProcTipo;
                    curProcTipo.Anexos.Add(result);
                }
                else
                    throw new Exception("Not supported type of Attachment.");

                var simpleCode = GetCodeSimple(code);
                this.Items.Add(StructTuple.Create(simpleCode, 0), result);
            }

            return result;
        }

        private ProcSubtipo CreateProcSubtipo(string code, string name, Pagina startPage, ProcTipo parentProcTipo)
        {
            var result = new ProcSubtipo
            {
                Cbhpm = this,
                Codigo = code,
                Nome = name,
                PaginaDeclarada = startPage,
                Tipo = parentProcTipo,
            };
            var simpleCode = GetCodeSimple(code);
            this.Items.Add(StructTuple.Create(simpleCode, 0), result);
            parentProcTipo.Subtipos.Add(result);
            return result;
        }

        private ProcTipo GetOrCreateProcTipo(string code, string name, Pagina startOrCurrentPage, Capitulo parentChapter)
        {
            name = name.ToUpperInvariant();

            var simpleCode = GetCodeSimple(code);
            bool createNew = false;
            ProcTipo found = null;
            object item;
            if (this.Items.TryGetValue(StructTuple.Create(simpleCode, 0), out item))
            {
                found = (ProcTipo)item;
                found = found.FindInSetByName(name) ?? found;
                if (found.Cbhpm != this) throw new Exception();
                if (found.Codigo != code) throw new Exception();
                if (found.Capitulo != parentChapter) throw new Exception();
                if (found.Nome != name)
                {
                    createNew = true;
                }
            }
            else
            {
                createNew = true;
            }

            ProcTipo result = null;
            if (createNew)
            {
                result = new ProcTipo(found)
                {
                    Cbhpm = this,
                    Codigo = code,
                    Nome = name,
                    Capitulo = parentChapter,
                    PaginaInicial = startOrCurrentPage,
                };

                parentChapter.ProcTipos.Add(result);

                this.Items.Add(StructTuple.Create(simpleCode, result.SetOfProcTipos.Count - 1), result);
            }
            else
            {
                result = found;
            }

            startOrCurrentPage.ProcTipo = result;

            return result;
        }

        private Pagina CreatePagina(int itPage, string pageText)
        {
            var result = new Pagina
            {
                Cbhpm = this,
                Numero = itPage,
                Texto = pageText,
            };
            this.Paginas.Add(result);
            return result;
        }

        private Capitulo CreateCapitulo(int num, string name, Pagina startPage)
        {
            var result = new Capitulo
            {
                Cbhpm = this,
                Numero = num,
                Nome = name,
                PaginaInicial = startPage,
            };
            this.Capitulos.Add(num, result);
            startPage.Capitulo = result;
            return result;
        }

        private static string GetCodeSimple(string code)
        {
            var match = Regex.Match(code, @"(\d\.\d\d\.\d\d\.\d\d)-\d");
            return match.Groups[1].Value;
        }

        public struct StructTuple
        {
            public static StructTuple<T1, T2> Create<T1, T2>(T1 value1, T2 value2)
            {
                return new StructTuple<T1, T2>(value1, value2);
            }
        }
        public struct StructTuple<T1, T2> : IEquatable<StructTuple<T1, T2>>
        {
            public StructTuple(T1 value1, T2 value2)
            {
                this.value1 = value1;
                this.value2 = value2;
            }

            private readonly T1 value1;
            private readonly T2 value2;

            private T1 Value1 { get { return this.value1; } }
            private T2 Value2 { get { return this.value2; } }

            public bool Equals(StructTuple<T1, T2> other)
            {
                return EqualityComparer<T1>.Default.Equals(this.Value1, other.Value1)
                    || EqualityComparer<T2>.Default.Equals(this.Value2, other.Value2);
            }

            public override bool Equals(object obj)
            {
                if (obj is StructTuple<T1, T2>)
                    return base.Equals((StructTuple<T1, T2>)obj);
                return false;
            }

            public override int GetHashCode()
            {
                return this.Value1.GetHashCode() | this.Value2.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("Tuple: [{0}, {1}]", this.Value1, this.Value2);
            }
        }

        private static void FinalizarPagina(Pagina curPagina)
        {
            // If previous page has no Procs, then clear the columns of that page.
            if (curPagina != null)
            {
                var cntProcsInPage = curPagina.GetProcs().Length;

                if (cntProcsInPage == 0)
                {
                    curPagina.Colunas.Clear();
                    curPagina.Colunas.Capacity = 0;
                }
                else
                {
                    var sequences = new[]
                        {
                                new string[] { },
                                new[] { "código", "procedimentos", "porte" },
                                new[] { "código", "procedimentos", "porte", "custo oper" },
                                new[] { "código", "procedimentos", "porte", "custo oper", "porte anest" },
                                new[] { "código", "procedimentos", "porte", "custo oper", "nº de aux" },
                                new[] { "código", "procedimentos", "porte", "custo oper", "nº de aux", "porte anest" },
                                new[] { "código", "procedimentos", "ur", "filme ou doc", "porte", "custo oper" },
                                new[] { "código", "procedimentos", "inc", "filme ou doc", "porte", "custo oper" },
                                new[] { "código", "procedimentos", "inc", "filme ou doc", "porte", "custo oper", "nº de aux", "porte anest" },
                            };

                    bool isColumnsValid = curPagina.FixColumnsOrder(sequences);

                    if (!isColumnsValid)
                        throw new Exception("Invalid sequence of columns.");
                }
            }
        }

        public override string ToString()
        {
            return string.Format("CBHPM - {0}", this.DataVersao);
        }

        public class Pagina
        {
            public Pagina()
            {
                this.Colunas = new List<string>();
            }

            public string Texto { get; set; }
            public int? Numero { get; set; }

            public Cbhpm Cbhpm { get; set; }
            public Capitulo Capitulo { get; set; } 
            public ProcTipo ProcTipo { get; set; }

            public List<string> Colunas { get; private set; }

            public override string ToString()
            {
                StringBuilder indiceProcs = new StringBuilder();

                foreach (var eachIndiceProc in this.IndiceProcedimentos())
                {
                    var types = new List<Type> { typeof(Cbhpm), typeof(Capitulo), typeof(ProcTipo), typeof(ProcSubtipo), typeof(Proc) };
                    var pos = types.IndexOf(eachIndiceProc.GetType());
                    var marker = new List<string> { "", "● ", "► ", "▪ ", "- " };
                    indiceProcs.AppendLine(new string(' ', pos * 2) + marker[pos] + eachIndiceProc.ToString());
                }

                return string.Format("Pg. {0}\n{1}", this.Numero, indiceProcs);
            }

            public List<object> IndiceProcedimentos()
            {
                var result = new List<object>();

                HashSet<object> usedObjects = new HashSet<object>();
                var procs = this.GetProcs();
                foreach (var eachProc in procs)
                {
                    if (!usedObjects.Contains(this.Cbhpm))
                        result.Add(this.Cbhpm);
                    usedObjects.Add(this.Cbhpm);

                    if (!usedObjects.Contains(this.Capitulo))
                        result.Add(this.Capitulo);
                    usedObjects.Add(this.Capitulo);

                    if (!usedObjects.Contains(this.ProcTipo))
                        result.Add(this.ProcTipo);
                    usedObjects.Add(this.ProcTipo);

                    if (!usedObjects.Contains(eachProc.Subtipo))
                        result.Add(eachProc.Subtipo);
                    usedObjects.Add(eachProc.Subtipo);

                    if (!usedObjects.Contains(eachProc))
                        result.Add(eachProc);
                    usedObjects.Add(eachProc);
                }

                return result;
            }

            public Proc[] GetProcs()
            {
                return this.Cbhpm.Items.Values.OfType<Proc>()
                    .Where(p => p.PaginaDeclarada == this)
                    .ToArray();
            }

            public void ColunaAdd(string colName)
            {
                colName = colName.ToLowerInvariant();
                string[][] duplas = new string[][]
                {
                    new string[] { "custo", "oper" },
                    new string[] { "nº de", "aux" },
                    new string[] { "porte", "anest" },
                    new string[] { "filme", "ou doc" },
                };
                foreach (var eachDupla in duplas)
                {
                    if (colName == eachDupla[1])
                    {
                        var index = this.Colunas.IndexOf(eachDupla[0]);
                        this.Colunas.RemoveAt(index);
                        colName = string.Join(" ", eachDupla);
                        break;
                    }
                }
                this.Colunas.Add(colName);
            }

            public bool FixColumnsOrder(string[][] sequences)
            {
                foreach (var eachSeq in sequences)
                {
                    if (this.Colunas.Count == eachSeq.Length
                        && this.Colunas.SequenceEqual(eachSeq, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public class Capitulo : ICbhpmItem
        {
            public Capitulo()
            {
                this.ProcTipos = new List<ProcTipo>();
                this.ProcTiposByCodigo = new Dictionary<string, ProcTipo>();
                this.Anexos = new List<TextoAnexo>();
            }

            public string Nome { get; set; }
            public int Numero { get; set; }

            public Cbhpm Cbhpm { get; set; }
            public Pagina PaginaInicial { get; set; }
            public List<ProcTipo> ProcTipos { get; private set; }
            public Dictionary<string, ProcTipo> ProcTiposByCodigo { get; private set; }
            public List<TextoAnexo> Anexos { get; private set; }

            public ICbhpmItem Parent
            {
                get { return null; }
            }

            public override string ToString()
            {
                return string.Format("Capítulo {0} - {1}", this.Numero, this.Nome);
            }

            public string Codigo
            {
                get { return string.Format("{0}", this.Numero); }
            }
        }

        public class ProcTipo : ICbhpmItem
        {
            public ProcTipo(ProcTipo otherProcTipoInSet = null)
            {
                this.Subtipos = new List<ProcSubtipo>();
                this.Anexos = new List<TextoAnexo>();

                // Adding ProcTipo to the set of items that share the same code.
                this.SetOfProcTipos = otherProcTipoInSet == null ? new HashSet<ProcTipo>() : otherProcTipoInSet.SetOfProcTipos;

                this.SetOfProcTipos.Add(this);
            }

            public Cbhpm Cbhpm { get; set; }
            public string Codigo { get; set; }
            public string Nome { get; set; }
            public Capitulo Capitulo { get; set; }
            public Pagina PaginaInicial { get; set; }

            public List<ProcSubtipo> Subtipos { get; private set; }
            public List<TextoAnexo> Anexos { get; private set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", this.Codigo, this.Nome);
            }

            public HashSet<ProcTipo> SetOfProcTipos { get; set; }

            public ProcTipo FindInSetByName(string name)
            {
                return this.SetOfProcTipos.FirstOrDefault(pt => pt.Nome == name);
            }

            public ICbhpmItem Parent
            {
                get { return this.Capitulo; }
            }
        }

        public class ProcSubtipo : ICbhpmItem
        {
            public ProcSubtipo()
            {
                this.Procedimentos = new List<Proc>();
                this.Anexos = new List<TextoAnexo>();
            }

            public Cbhpm Cbhpm { get; set; }
            public string Codigo { get; set; }
            public string Nome { get; set; }
            public ProcTipo Tipo { get; set; }

            public List<Proc> Procedimentos { get; private set; }
            public List<TextoAnexo> Anexos { get; private set; }
            public Pagina PaginaDeclarada { get; set; }

            public ICbhpmItem Parent
            {
                get { return this.Tipo; }
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", this.Codigo, this.Nome);
            }
        }

        public class Proc : ICbhpmItem
        {
            public Proc()
            {
            }

            public Cbhpm Cbhpm { get; set; }
            public string Codigo { get; set; }
            public string Nome { get; set; }
            public string Porte { get; set; }
            public string NumAux { get; set; }
            public string PorteAnest { get; set; }
            public string CustoOper { get; set; }
            public string Ur { get; set; }
            public string Inc { get; set; }
            public string FilmeOuDoc { get; set; }
            public ProcSubtipo Subtipo { get; set; }
            public string GrupoNoSubtipo { get; set; }
            public Pagina PaginaDeclarada { get; set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", this.Codigo, this.Nome);
            }

            public ICbhpmItem Parent
            {
                get { return this.Subtipo; }
            }
        }

        public class TextoAnexo : ICbhpmItem
        {
            public Cbhpm Cbhpm { get; set; }
            public string Codigo { get; set; }
            public string Nome { get; set; }
            public string Texto { get; set; }
            public Pagina PaginaDeclarada { get; set; }

            public ProcSubtipo Subtipo { get; set; }
            public ProcTipo Tipo { get; set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", this.Codigo, this.Nome);
            }

            public Capitulo Capitulo { get; set; }

            public ICbhpmItem Parent
            {
                get { return this.Subtipo; }
            }
        }

        //public class Codigo : IEquatable<Codigo>
        //{
        //    byte capitulo;
        //    byte tipo;
        //    byte subtipo;
        //    byte id;
        //    byte seq;

        //    private Codigo()
        //    {
        //    }

        //    private Codigo(string codigo)
        //    {
        //        var match = Regex.Match(codigo, @"^(\d)\.(\d\d)\.(\d\d)\.(\d\d)-(\d)$");
        //        this.capitulo = byte.Parse(match.Groups[1].Value);
        //        this.tipo = byte.Parse(match.Groups[2].Value);
        //        this.subtipo = byte.Parse(match.Groups[3].Value);
        //        this.id = byte.Parse(match.Groups[4].Value);
        //        this.seq = byte.Parse(match.Groups[5].Value);
        //    }

        //    public Codigo Tipo
        //    {
        //        get
        //        {
        //            return new Codigo { capitulo = this.capitulo, tipo = this.tipo };
        //        }
        //    }

        //    public Codigo Subtipo
        //    {
        //        get
        //        {
        //            return new Codigo { capitulo = this.capitulo, tipo = this.tipo };
        //        }
        //    }

        //    public override string ToString()
        //    {
        //        return string.Format("{0}.{1:00}.{2:00}.{3:00}-{4}", this.capitulo, this.tipo, this.subtipo, this.id, this.seq);
        //    }

        //    public virtual bool Equals(Codigo other)
        //    {
        //        if (other == null)
        //            return false;
        //        return StringComparer.Ordinal.Equals(this.ToString(), other.ToString());
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        return this.Equals(obj as Codigo);
        //    }

        //    public override int GetHashCode()
        //    {
        //        return this.ToString().GetHashCode();
        //    }
        //}

        public interface ICbhpmItem
        {
            string Codigo { get; }
            string Nome { get; }
            ICbhpmItem Parent { get; }
        }
    }
}
