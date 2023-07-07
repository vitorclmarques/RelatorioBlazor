using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport;
using FastReport.Export.Html;
using FastReport.Web;
using FastReport.Data;
using FastReport.Dialog;
using FastReport.Barcode;
using FastReport.Table;
using FastReport.Utils;
using Microsoft.AspNetCore.Mvc;
using NotesFor.HtmlToOpenXml;
using Npgsql;
using RelatorioBlazor.Shared;
using SelectPdf;
using Parameter = FastReport.Data.Parameter;

namespace ContabilidadeOrienteSistemas.Server.CRUD
{
    [Route("[controller]")]
    [ApiController]
    public class RelatorioController : ControllerBase
    {

        NpgsqlConnection conexao = new() { ConnectionString = @"Host=DESENV02; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };

        [HttpGet("Relatorio/ExecutarComandoSql/{Comando}")]
        public async Task<object> ExecutarComandoSql(string comando)
        {
            try
            {
                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = comando;

                DbDataReader dados = cmd.ExecuteReader();

                DataTable datatable = new();
                datatable.Load(dados);

                return null; //JsonConvert.SerializeObject(datatable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] ExecutarComandoSql {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }


        [HttpPost("GerarRelatorio")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> GerarRelatorioPreCarregado([FromBody] CorpoRequisicaoRelatorio requisicao )
        {
            try
            {
                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(carregarModeloRelatorio(0));


                if (requisicao.parametrosRelatorio is not null && requisicao.parametrosRelatorio.Count != 0)
                {
                    foreach (var item in requisicao.parametrosRelatorio)
                    {
                        relatorio1.Report.SetParameterValue("QtdVT", 10);
                        relatorio1.Report.SetParameterValue("VlrVt", 4.60);

                    }
                }




                WebReport relatorio2 = new WebReport();
                relatorio2.Report.Load(carregarModeloRelatorio(1));
                if (requisicao.parametrosRelatorio is not null && requisicao.parametrosRelatorio.Count != 0)
                {
                    foreach (var item in requisicao.parametrosRelatorio)
                    {
                        relatorio2.Report.SetParameterValue(item.Key, item.Value);
                    }
                }

                //Faça o SQL aqui

                conexao.Open();

                Npgsql.NpgsqlCommand comando = new("SELECT hc.nome AS colaborador, con.nome AS contribuinte, con.inscricao AS cnpj, ec.logradouro AS enderecocolab, ec.cep, ec.numero, ec.bairro, d.nome AS departamento, c.cpf AS cpf " +
                                                    "FROM pessoal.colaborador AS c " +
                                                    "INNER JOIN pessoal.hcolaboradorcadastral AS hc ON c.id = hc.IdColaborador " +
                                                    "INNER JOIN oriente.endereco AS ec ON hc.idcolaborador = ec.idorigem " +
                                                    "INNER JOIN oriente.contribuinte AS con ON hc.IdColaborador = con.id " +
                                                    "LEFT JOIN pessoal.vinculo" +
                                                    "departamento AS v ON v.idcolaborador = c.Id " +
                                                    "LEFT JOIN pessoal.departamento AS d ON v.iddepartamento = d.idcontribuinte " +
                                                    "WHERE c.codigo = 10 AND ec.tipo = 2", conexao);


                List<object> listaDados = new List<object>();
                var leitor = comando.ExecuteReader();

                while (leitor.Read())
                {
                    var dados = new
                    {
                        colaborador = leitor.GetString(leitor.GetOrdinal("Colaborador")),
                        contribuinte = leitor.GetString(leitor.GetOrdinal("contribuinte")),
                        cnpj = leitor.GetString(leitor.GetOrdinal("CNPJ")),
                        enderecocolab = leitor.GetString(leitor.GetOrdinal("enderecoColab")),
                        cep = leitor.GetString(leitor.GetOrdinal("cep")),
                        numero = leitor.GetString(leitor.GetOrdinal("numero")),
                        bairro = leitor.GetString(leitor.GetOrdinal("bairro")),
                        departamento = leitor.GetString(leitor.GetOrdinal("departamento")),
                        cpf = leitor.GetString(leitor.GetOrdinal("cpf")),
                    };
                        listaDados.Add(dados);
                }
              
        
            





        //Exemplo incompleto de Parametros de SQL, precisa formatar (colocar '', AND, OR ou outros elementos)
        StringBuilder sql = new();
/*
                if (requisicao.parametrosSql is not null && requisicao.parametrosSql.Count != 0)
                {
                    sql.Append("WHERE");

                    foreach (var item in requisicao.parametrosSql)
                    {
                        sql.Append($" {item.Key} = {item.Value}");
                    }
                }*/

                string texto = sql.ToString();

                //SQL

                //O SQL traz uma lista a ser pós processada
            /*    List<Tabela> Tabela = new()
                {
                    new("macaco", DateTime.Now, 1),
                    new("cachorro", DateTime.Now.AddDays(3), 3),
                    new("gato", DateTime.Now.AddDays(2), 2),
                    new("pato", DateTime.Now.AddDays(1), 1),
                };
*/
                //Teste 1 --- List<Tuple> Não Funcional, Erro no RegisterData ---
               /* List<(string Animal, DateTime Data, int Contagem)> TabelaX = new()
                {
                    new("macaco", DateTime.Now, 1),
                    new("cachorro", DateTime.Now.AddDays(3), 3),
                    new("gato", DateTime.Now.AddDays(2), 2),
                    new("pato", DateTime.Now.AddDays(1), 1),
                };*/

                //Teste 2 --- List<AnomObject> Funcional!!!
              /*  var TabelaY = new[] {
                    new { Animal = "macaco", Data = DateTime.Now, Contagem = 1},
                    new { Animal = "cachorro", Data = DateTime.Now.AddDays(3), Contagem = 3},
                    new { Animal = "gato", Data = DateTime.Now.AddDays(2), Contagem = 2},
                    new { Animal = "pato", Data = DateTime.Now.AddDays(1), Contagem = 1}
                    };*/
               /* var TabelaW = new[] {
                    new { Animal = "macaco", Data = DateTime.Now, Contagem = 1},
                    new { Animal = "cachorro", Data = DateTime.Now.AddDays(3), Contagem = 3},
                    new { Animal = "gato", Data = DateTime.Now.AddDays(2), Contagem = 2},
                    new { Animal = "pato", Data = DateTime.Now.AddDays(1), Contagem = 1}
                    };
*/

               

               /* while (leitor.Read())
                {
                    var dados = new
                    {
                        Colaborador = leitor.GetString(leitor.GetOrdinal("Colaborador")),
                        Contribuinte = leitor.GetString(leitor.GetOrdinal("contribuinte")),
                        CNPJ = leitor.GetString(leitor.GetOrdinal("CNPJ")),
                        EnderecoColaborador = leitor.GetString(leitor.GetOrdinal("enderecoColab")),
                        CEP = leitor.GetString(leitor.GetOrdinal("cep")),
                        Numero = leitor.GetString(leitor.GetOrdinal("numero")),
                        Bairro = leitor.GetString(leitor.GetOrdinal("bairro")),
                        Departamento = leitor.GetString(leitor.GetOrdinal("DEPART")),
                        CPF = leitor.GetString(leitor.GetOrdinal("CPF"))
                    };

                    listaDados.Add(dados);
                }*/





                //Teste de Prova
                //var anonymousObject = new { Name = "John", Age = 30 };
                //string jsonString = JsonSerializer.Serialize(anonymousObject);                

                /* Teste caso mude de BusinessObject para Json
                var jsonTexto = JsonSerializer.Serialize(Tabela);
                var jsonTexto1 = JsonSerializer.Serialize(TabelaX); // Ñ OK Retorna Vazio
                var jsonTexto2 = JsonSerializer.Serialize(TabelaY); // OK   Retorna Preenchido
                */

                //Lista do tipo Nome do BusinessObject

                /*     relatorio1.Report.RegisterData(TabelaY, "Tabela");*/

                /*  relatorio1.Report.Prepare();*/

                string html = GerarHtml(relatorio1);

                


                //return Ok(html);

                relatorio2.Report.RegisterData(listaDados, "Table1");
                relatorio2.Report.Prepare();


                string html2 = GerarHtml(relatorio2);
           

                conexao.Close();

                return Ok(html2);





            }
            catch (Exception ex)
            {
                return BadRequest($"Erro Encontrado: [DBController] GeradorDeRelatorio {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        record Tabela (string Animal, DateTime Data, int Contagem);





        MemoryStream carregarModeloRelatorio(short tipo) =>
        tipo switch
        {
            //Adicione o relatório como Recurso na Pagina de Propriedades do Projeto

            0 => new(RelatorioBlazor.Server.Properties.Resources.teste),

            1 => new(RelatorioBlazor.Server.Properties.Resources.valeTransporte),
        };








        [HttpPost("GerarPDF")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> DownloadPDFGrande()
        {

            string texto = (await Request.ReadFromJsonAsync<string>());

            using MemoryStream ms = new MemoryStream();

            HtmlToPdf conversor = new();
            conversor.Options.PdfStandard = PdfStandard.PdfA;
            conversor.Options.PdfPageSize = PdfPageSize.A4;
            conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
            conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

            conversor.Options.MarginLeft = 20;
            conversor.Options.MarginTop = 20;
            conversor.Options.MarginRight = 20;
            conversor.Options.MarginBottom = 0;

            var a = texto.Length;

            //SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto.Replace("<br />",""));
            SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto.Replace("<br />", ""));

            pdf.Save(ms);

            return File(ms.ToArray(), "application/text");
        }


        string GerarHtml(WebReport relatorio)
        {
            using MemoryStream ms = new();

            HTMLExport html = new HTMLExport();
            html.EmbedPictures = true;
            html.SinglePage = true;
            html.SubFolder = false;
            html.Layers = true;
            html.Navigator = false;
            //html.ExportMode = HTMLExport.ExportType.WebPrint; //opriginal desativado

            relatorio.Report.Export(html, ms);

            string texto = Encoding.UTF8.GetString(ms.ToArray());
            
            return ConsertarPaginacaoHtml(texto, true);
            //return texto;

        }


        public string ConsertarPaginacaoHtml(string texto, bool espacoEntrePaginas = false)
        {

            StringBuilder sb = new();

            var linha = texto.Split("\n");

            string criterio = "PageN2";
            int criterioIndex = 2;

            for (int i = 0; i < linha.Length; i++)
            {
                if (linha[i].Contains(criterio))
                {
                    var linhaSplit = linha[i].Split('>');

                    linhaSplit[1] = espacoEntrePaginas ? "<a name=\"PageN" + criterioIndex + "\" style=\"padding:0;margin:0;font-size:1px;\"></a><div style=\"page-break-after: always; background-color: #E3E3E3; \"><br /></div"
                                                       : "<a name=\"PageN" + criterioIndex + "\" style=\"padding:0;margin:0;font-size:1px;\"></a><div style=\"page-break-after: always; background-color: #E3E3E3; \"></div";

                    linha[i] = string.Empty;

                    for (int j = 0; j < linhaSplit.Length; j++)
                    {
                        if (linhaSplit[j] == "\r") { linha[i] += linhaSplit[j]; }
                        else { linha[i] += linhaSplit[j] + ">"; }
                    }

                    sb.Append(linha[i]);

                    criterioIndex++;
                    criterio = "PageN" + criterioIndex.ToString();
                }
                else
                {
                    sb.Append(linha[i]);
                }
            }

            return sb.ToString();
        }



    }

}
        
        
        
        
        
        /*
                [HttpGet("GerarRelatorio")]
                public async Task<string> GerarRelatorio()
                {
                    try
                    {
                        conexao.Open();

                        DbCommand cmd = conexao.CreateCommand();
                        cmd.CommandText = "SELECT * FROM oriente.contabilidade.planocontabil";

                        DbDataReader dados = cmd.ExecuteReader();
                        PlanoContabil y = new();

                        //var z = JsonSerializer.Serialize(dados);

                        //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                        DataTable datatable = new();
                        datatable.Load(dados);



                        Report relatorio1 = new Report();
                        relatorio1.Load(new MemoryStream(Properties.Resources.teste));
                        relatorio1.RegisterData(datatable, "contabilidade_planocontabil");

                        ((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.GetDataSource("contabilidade_planocontabil");
                        relatorio1.GetDataSource("contabilidade_planocontabil").Enabled = true;

                        relatorio1.Prepare();



                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                        return null;
                    }
                }
        



        // Gera os relatório PlanoContabil, PlanoContabilComReferencial, Historico, Livro Diario
        [HttpGet("GerarRelatorio/{Tipo}/{Filtro}/{Parametro}")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<string>> GerarRelatorio(short tipo, string filtro, string parametro)
        {
            try
            {

                WebReport relatorio = new WebReport();
                relatorio.Report.Load(carregarModeloRelatorio((tipoRelatorio)tipo));

                if (!parametro.Contains("vazio"))
                {
                    Dictionary<string, object> colecaoParametro = parametro.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(part => part.Split('='))
                                                       .ToDictionary(split => split[0], split => (object)split[1]);

                    definirParametroRelatorio(relatorio, colecaoParametro);
                }

                definirDadosRelatorio(relatorio, (tipoRelatorio)tipo, filtro.Split("&"));

                relatorio.Report.Prepare();

                string html = GerarHtml(relatorio);

                return Ok(html);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro Encontrado: [DBController] GeradorDeRelatorio {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }



        [HttpPost("GerarRelatorio/{Tipo}/{Parametro}")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> GerarRelatorioPreCarregado(short tipo, string parametro)
        {
            try
            {
                object dados = null;

                switch ((tipoRelatorio)tipo)
                {
                    case tipoRelatorio.LivroCaixa:
                    case tipoRelatorio.LivroRazao:
                        dados = await Request.ReadFromJsonAsync<RelatorioPostRazao>();
                        break;

                    case tipoRelatorio.Balanco:
                        dados = await Request.ReadFromJsonAsync<RelatorioPostBalanco>();
                        break;

                    case tipoRelatorio.Balancete:
                        dados = await Request.ReadFromJsonAsync<RelatorioPostBalancete>();
                        break;

                    case tipoRelatorio.Demonstracoes:
                        dados = await Request.ReadFromJsonAsync<RelatorioPostBalanco>();
                        break;

                    case tipoRelatorio.NotaExplicativa:
                        dados = await Request.ReadFromJsonAsync<RelatorioPostNotaExplicativa>();

                        parametro += $"&Texto={((RelatorioPostNotaExplicativa)dados).DadosPrimario.Replace("<br>", "<br/>")}";

                        //dados = new List<RelatorioAssinatura>() { ((RelatorioPostNotaExplicativa)dados).DadosAssinatura };

                        break;

                    case tipoRelatorio.Termo:
                        dados = await Request.ReadFromJsonAsync<RelatorioPostNotaExplicativa>();

                        parametro += $"&Texto={((RelatorioPostNotaExplicativa)dados).DadosPrimario.Replace("<br>", "<br/>")}";


                        break;

                    case tipoRelatorio.Importacao:
                        dados = await Request.ReadFromJsonAsync<RelatorioImportacao>();
                        

                        break;

                    case tipoRelatorio.PlanoContabil:
                    case tipoRelatorio.PlanoContabilComReferencial:
                    case tipoRelatorio.HistoricoContabil:
                    case tipoRelatorio.LivroDiario:
                    case tipoRelatorio.AuxRelatorioSaldoPeriodico:
                    default:
                        break;
                }

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(carregarModeloRelatorio((tipoRelatorio)tipo));

                if (!parametro.Contains("vazio"))
                {


                    Dictionary<string, object> colecaoParametro = parametro.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(part => part.Split('='))
                                                       .ToDictionary(split => split[0], split => (object)split[1]);

                    definirParametroRelatorio(relatorio1, colecaoParametro);
                }

                //var a = relatorio1.Report.Parameters.ToArray();

                definirDadosRelatorio(relatorio1, (tipoRelatorio)tipo, dados);

                relatorio1.Report.Prepare();

                string html = GerarHtml(relatorio1);

                return Ok(html);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro Encontrado: [DBController] GeradorDeRelatorio {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }






        string GerarHtml(WebReport relatorio)
        {
            using MemoryStream ms = new();

            HTMLExport html = new HTMLExport();
            html.EmbedPictures = true;
            html.SinglePage = true;
            html.SubFolder = false;
            html.Layers = true;
            html.Navigator = false;
            //html.ExportMode = HTMLExport.ExportType.WebPrint; //opriginal desativado

            relatorio.Report.Export(html, ms);

            string texto = Encoding.UTF8.GetString(ms.ToArray());

            return utilContabilidade.ConsertarPaginacaoHtml(texto, true);
            //return texto;

        }



        string consertarHtml(string texto)
        {

            StringBuilder sb = new();

            var linha = texto.Split("\n");

            string criterio = "PageN2";
            int criterioIndex = 2;
            bool umaVez = true;

            for (int i = 0; i < linha.Count(); i++)
            {
                var temp = linha[i];

                if (linha[i].Contains(criterio))
                {
                    var linhaSplit = linha[i].Split('>');

                    linhaSplit[1] = "<a name=\"PageN" + criterioIndex + "\" style=\"padding:0;margin:0;font-size:1px;\"></a><div style=\"page-break-after: always\"></div";

                    var temp1 = linha[i];

                    linha[i] = string.Empty;

                    for (int j = 0; j < linhaSplit.Count(); j++)
                    {
                        if (linhaSplit[j] == "\r")
                        {
                            linha[i] += linhaSplit[j];
                        }
                        else
                        {
                            linha[i] += linhaSplit[j] + ">";
                        }

                    }

                    sb.Append(linha[i]);

                    criterioIndex++;
                    criterio = "PageN" + criterioIndex.ToString();
                }
                else
                {
                    sb.Append(linha[i]);
                }
            }

            return sb.ToString();
        }



        MemoryStream carregarModeloRelatorio(tipoRelatorio tipo) =>
        tipo switch
        {
            tipoRelatorio.PlanoContabil => new(Properties.Resources.PlanoContabilConta),
            tipoRelatorio.PlanoContabilComReferencial => new(Properties.Resources.PlanoContabilContaComReferencial),
            tipoRelatorio.HistoricoContabil => new(Properties.Resources.HistoricoContabil),
            tipoRelatorio.LivroDiario => new(Properties.Resources.LivroDiario),
            tipoRelatorio.LivroRazao => new(Properties.Resources.LivroRazao),
            tipoRelatorio.LivroCaixa => new(Properties.Resources.LivroCaixa),
            tipoRelatorio.Demonstracoes => new(Properties.Resources.DRE),
            tipoRelatorio.Balanco => new(Properties.Resources.Balanco),
            tipoRelatorio.Balancete => new(Properties.Resources.Balancete),
            tipoRelatorio.NotaExplicativa => new(Properties.Resources.NotaExplicativa),
            tipoRelatorio.Termo => new(Properties.Resources.Termo),
            tipoRelatorio.Importacao => new(Properties.Resources.Importacao),
        };



        static void definirParametroRelatorio(WebReport relatorio, Dictionary<string, object> colecaoParametro)
        {
            for (int i = 0; i < colecaoParametro.Count(); i++)
            {

                switch (colecaoParametro.Keys.ElementAt(i))
                {
                    case string a when a.Contains("Inscricao"):
                        relatorio.Report.SetParameterValue(colecaoParametro.Keys.ElementAt(i), colecaoParametro.Values.ElementAt(i).ToString().FormatarInscricao());
                        break;
                    
                    case string b when b.Contains("Periodo"):
                    case string c when c.Contains("CabecalhoAdicional"):
                    case string d when d.Contains("Data"):
                        relatorio.Report.SetParameterValue(colecaoParametro.Keys.ElementAt(i), colecaoParametro.Values.ElementAt(i).ToString().Replace("-", "/"));
                        break;
                                           
                    case string e when e.StartsWith("Tem"):
                        relatorio.Report.SetParameterValue(colecaoParametro.Keys.ElementAt(i), bool.Parse(colecaoParametro.Values.ElementAt(i).ToString()));
                        break;

                    case string f when f.Contains("PaginaInicial"):
                        relatorio.Report.InitialPageNumber = int.Parse(colecaoParametro.Values.ElementAt(i).ToString());
                        break;

                    case string g when g.Contains("Numeracao"):
                        relatorio.Report.SetParameterValue(colecaoParametro.Keys.ElementAt(i), int.Parse(colecaoParametro.Values.ElementAt(i).ToString()));
                        break;

                    case string h when h.Contains("Double"):
                        relatorio.Report.SetParameterValue(colecaoParametro.Keys.ElementAt(i), double.Parse(colecaoParametro.Values.ElementAt(i).ToString()));
                        break;

                    default:
                        relatorio.Report.SetParameterValue(colecaoParametro.Keys.ElementAt(i), colecaoParametro.Values.ElementAt(i));
                        break;
                }
            }
        }



        void definirDadosRelatorio(WebReport relatorio, tipoRelatorio tipo, string[] filtro)
        {
#if !DEBUG
                conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

            conexao.Open();

            DbCommand cmd = conexao.CreateCommand();
            cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipo, filtro);

            DbDataReader dados = cmd.ExecuteReader();

            DataTable datatable = new();
            datatable.Load(dados);

            if (datatable.Rows.Count == 0
                && tipo != tipoRelatorio.LivroDiario
                && tipo != tipoRelatorio.LivroRazao) { throw new ArgumentOutOfRangeException($"O comando retornou sem nenhum dados. {Environment.NewLine} Tipo: {tipo.ToString()}{Environment.NewLine} Comando SQL: {cmd.CommandText}"); }

            relatorio.Report.RegisterData(datatable, "Tabela");
        }




        void definirDadosRelatorio(WebReport relatorio, tipoRelatorio tipo, object dados)
        {
            switch (tipo)
            {
                case tipoRelatorio.NotaExplicativa:
                    //relatorio.Report.RegisterData((dados as List<RelatorioAssinatura>), "Assinatura");
                    relatorio.Report.RegisterData(new List<RelatorioAssinatura>() { (dados as RelatorioPostNotaExplicativa).DadosAssinatura }, "Assinatura");
                    break;

                case tipoRelatorio.Termo:
                    relatorio.Report.RegisterData(new List<RelatorioTermo>() { (dados as RelatorioPostNotaExplicativa).DadosSecundario }, "Termo");
                    relatorio.Report.RegisterData(new List<RelatorioAssinatura>() { (dados as RelatorioPostNotaExplicativa).DadosAssinatura }, "Assinatura");                    
                    break;

                case tipoRelatorio.LivroCaixa:
                case tipoRelatorio.LivroRazao:
                    relatorio.Report.RegisterData((dados as RelatorioPostRazao).DadosPrimario, "Tabela");
                    relatorio.Report.RegisterData((dados as RelatorioPostRazao).DadosSecundario, "Grupo");

                    break;

                case tipoRelatorio.Balanco:
                    relatorio.Report.RegisterData((dados as RelatorioPostBalanco).DadosPrimario, "Tabela");
                    relatorio.Report.RegisterData((dados as RelatorioPostBalanco).DadosSecundario, "Tabela2");
                    relatorio.Report.RegisterData(new List<RelatorioAssinatura>() { (dados as RelatorioPostBalanco).DadosAssinatura }, "Assinatura");

                    break;

                case tipoRelatorio.Balancete:
                    relatorio.Report.RegisterData((dados as RelatorioPostBalancete).DadosPrimario, "Tabela");
                    relatorio.Report.RegisterData((dados as RelatorioPostBalancete).DadosSecundario, "Tabela2");
                    relatorio.Report.RegisterData(new List<RelatorioAssinatura>() { (dados as RelatorioPostBalancete).DadosAssinatura }, "Assinatura");
                    break;

                case tipoRelatorio.Demonstracoes:
                    relatorio.Report.RegisterData((dados as RelatorioPostBalanco).DadosPrimario, "Tabela");
                    relatorio.Report.RegisterData((dados as RelatorioPostBalanco).DadosSecundario, "Tabela2");
                    relatorio.Report.RegisterData((dados as RelatorioPostBalanco).DadosTerceario, "Tabela3");
                    relatorio.Report.RegisterData((dados as RelatorioPostBalanco).DadosQuarternario, "Tabela4");
                    relatorio.Report.RegisterData(new List<RelatorioAssinatura>() { (dados as RelatorioPostBalanco).DadosAssinatura }, "Assinatura");

                    break;

                case tipoRelatorio.Importacao:
                    //relatorio.Report.RegisterData((dados as RelatorioImportacao).DadosPessoa, "Pessoa");
                    relatorio.Report.RegisterData((dados as RelatorioImportacao).DadosPlanoContabil, "PlanoConta");
                    relatorio.Report.RegisterData((dados as RelatorioImportacao).DadosPlanoContabilContas, "PlanoContaContabil");
                    relatorio.Report.RegisterData((dados as RelatorioImportacao).DadosSaldos, "Saldo");
                    relatorio.Report.RegisterData((dados as RelatorioImportacao).DadosLancamento, "Lancamento");

                    break;

                default:
                    break;
            }
        }



        double calcularSaldoInicial(string codigoCompleto, List<ObjetoSqlBalancante> lista)
        {
            double valorDebito = lista.Where(x => x.Debito.StartsWith(codigoCompleto) & x.Credito.StartsWith("0")).Sum(x => x.Total);
            double valorCredito = lista.Where(x => x.Credito.StartsWith(codigoCompleto) & x.Debito.StartsWith("0")).Sum(x => x.Total);

            return valorDebito - valorCredito;
        }


        #region MyRegion



        //PlanoContabil / PlanoContabilComReferencial (não implementado) / Historico (não implementado)
        [HttpGet("GerarRelatorio")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorio()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = "SELECT pc.codigocompleto, pc.codigoreduzido, pc.nome, pc.tipo, pc.naturezasped, " +
                                  "CHARACTER_LENGTH(pc.codigocompleto) - CHARACTER_LENGTH(REPLACE(pc.codigocompleto, '.', '' )) + 1 AS nivel, " +
                                  "CASE WHEN (CHARACTER_LENGTH(p.mascara) - CHARACTER_LENGTH(REPLACE(p.mascara, '.', '' )) + 1 - (CHARACTER_LENGTH(pc.codigocompleto) - CHARACTER_LENGTH(REPLACE(pc.codigocompleto, '.', '' )) + 1)) = 0 THEN 'Análitica' ELSE 'Sintética' END AS tipoconta " +
                                  "FROM contabilidade.\"planocontabilconta\" pc " +
                                  "INNER JOIN contabilidade.\"planocontabil\" p ON p.id = pc.planocontabilid WHERE pc.planocontabilid = 1";

                DbDataReader dados = cmd.ExecuteReader();
                PlanoContabil y = new();

                //var z = JsonSerializer.Serialize(dados);

                //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                DataTable datatable = new();
                datatable.Load(dados);

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.PlanoContabilConta));

                relatorio1.Report.SetParameterValue("NomeEmpresa", "Teste");
                relatorio1.Report.SetParameterValue("InscricaoEmpresa", "00.000.000/0000-00");

                //relatorio1.Report.Dictionary.Connections[0].ConnectionString = "Host=Estu00;Username=oriente;Password=sistemas;Database=oriente;Port=23286";

                relatorio1.Report.RegisterData(datatable, "Tabela");

                ((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Tabela");
                relatorio1.Report.GetDataSource("Tabela").Enabled = true;

                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);


                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                var ax = utilContabilidade.ConsertarPaginacaoHtml(Encoding.UTF8.GetString(ms.ToArray()), true);

                ObjetoRelatorio rel = new() { PaginaHtml = utilContabilidade.ConsertarPaginacaoHtml(Encoding.UTF8.GetString(ms.ToArray()), true), ArquivoPDF = ms1.ToArray() };

                return Ok(rel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }


        //Teste
        [HttpGet("GerarRelatorioTeste")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioX()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = "SELECT pc.codigocompleto, pc.codigoreduzido, pc.nome, pc.tipo, pc.naturezasped, " +
                                  "CHARACTER_LENGTH(pc.codigocompleto) - CHARACTER_LENGTH(REPLACE(pc.codigocompleto, '.', '' )) + 1 AS nivel, " +
                                  "CASE WHEN (CHARACTER_LENGTH(p.mascara) - CHARACTER_LENGTH(REPLACE(p.mascara, '.', '' )) + 1 - (CHARACTER_LENGTH(pc.codigocompleto) - CHARACTER_LENGTH(REPLACE(pc.codigocompleto, '.', '' )) + 1)) = 0 THEN 'Análitica' ELSE 'Sintética' END AS tipoconta " +
                                  "FROM contabilidade.\"planocontabilconta\" pc " +
                                  "INNER JOIN contabilidade.\"planocontabil\" p ON p.id = pc.planocontabilid WHERE pc.planocontabilid = 1";

                DbDataReader dados = cmd.ExecuteReader();
                PlanoContabil y = new();

                //var z = JsonSerializer.Serialize(dados);

                //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                DataTable datatable = new();
                datatable.Load(dados);

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.teste1));

                relatorio1.Report.SetParameterValue("Teste1", "Teste");
                relatorio1.Report.SetParameterValue("Teste2", "00.000.000/0000-00");
                relatorio1.Report.SetParameterValue("Teste3", "Concluido");
                relatorio1.Report.InitialPageNumber = 99;

                //relatorio1.Report.Dictionary.Connections[0].ConnectionString = "Host=Estu00;Username=oriente;Password=sistemas;Database=oriente;Port=23286";

                //relatorio1.Report.RegisterData(datatable, "Tabela");

                //((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Tabela");
                //relatorio1.Report.GetDataSource("Tabela").Enabled = true;

                ((DataBand)relatorio1.Report.FindObject("Data1")).Visible = true;
                ((DataBand)relatorio1.Report.FindObject("Data2")).Visible = true;
                ((DataBand)relatorio1.Report.FindObject("Data3")).Visible = true;

                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };

                return Ok(rel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }

        //Teste
        [HttpGet("GerarRelatorioTeste2")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioY()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = "SELECT * FROM oriente.contabilidade.lancamentocontabil ";

                DbDataReader dados = cmd.ExecuteReader();
                PlanoContabil y = new();

                //var z = JsonSerializer.Serialize(dados);

                //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                DataTable datatable = new();
                datatable.Load(dados);

                datatable.Columns.Add("coluna1", typeof(Int32));
                datatable.Columns.Add("coluna2", typeof(Int32));


                Random rnd = new(1);


                var json = "{\"$id\": \"https://xxxxxx.com/person.schema.json\", \"$schema\": \"https://json-schema.org/draft/2020-12/schema\", \"title\": \"Hello Rato World!\", \"type\": \"objectu\", \"properties\": { }, \"description\": \"My segundo JSON Schema.\"}";


                foreach (DataRow row in datatable.Rows)
                {
                    //need to set value to NewColumn column
                    row["coluna1"] = rnd.Next(1, 10); ;   // or set it to some other value
                    row["coluna2"] = rnd.Next(1, 10); ;   // or set it to some other value
                }

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.teste2));


                //TesteJson t1 = new() { title = "teste 11", description = "só x teste", id = 11 };
                //TesteJson t2 = new() { title = "teste 22", description = "só y teste", id = 22 };
                //TesteJson t3 = new() { title = "teste 33", description = "só z teste", id = 33 };

                /*

                                List<string> tx = new();
                                tx.Add(JsonConvert.SerializeObject(t1));
                                tx.Add(JsonConvert.SerializeObject(t2));
                                tx.Add(JsonConvert.SerializeObject(t3));

                                var txx = JsonConvert.SerializeObject(tx);

                                //List<TesteJson> ty = new();
                                //ty.Add(t1);
                                //ty.Add(t2);
                                //ty.Add(t3);


                                Type entityType = typeof(TesteJson);
                                DataTable table = new DataTable(entityType.Name);
                                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
                                foreach (PropertyDescriptor prop in properties)
                                {
                                    table.Columns.Add(prop.Name, prop.PropertyType);
                                }


                                foreach (TesteJson item in ty)
                                {
                                    DataRow row = table.NewRow();
                                    foreach (PropertyDescriptor prop in properties)
                                    {
                                        row[prop.Name] = prop.GetValue(item);
                                    }
                                    table.Rows.Add(row);
                                }



                                relatorio1.Report.RegisterData(table, "JSON.item");



                                ((DataBand)relatorio1.Report.FindObject("Data2")).DataSource = relatorio1.Report.GetDataSource("JSON.item");
                                relatorio1.Report.GetDataSource("JSON").Enabled = true;

                                relatorio1.Report.Prepare();

                                MemoryStream ms = new();
                                MemoryStream ms1 = new();

                                HTMLExport html = new HTMLExport();
                                // We need embedded pictures inside html
                                html.EmbedPictures = true;
                                // Enable all report pages in one html file
                                html.SinglePage = false;
                                // We don't need a subfolder for pictures and additional files
                                html.SubFolder = false;
                                // Enable layered HTML
                                html.Layers = true;
                                // Turn off the toolbar with navigation
                                html.Navigator = false;
                                // Save the report in html
                                relatorio1.Report.Export(html, ms);

                                string texto = Encoding.UTF8.GetString(ms.ToArray());

                                texto = consertarHtml(texto);



                                HtmlToPdf conversor = new();
                                conversor.Options.PdfStandard = PdfStandard.PdfA;
                                conversor.Options.PdfPageSize = PdfPageSize.A4;
                                //conversor.Options.PdfPageCustomSize = new(595, 842);
                                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                                conversor.Options.MarginLeft = 20;
                                conversor.Options.MarginTop = 20;
                                conversor.Options.MarginRight = 20;
                                conversor.Options.MarginBottom = 20;

                                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                                pdf.Save(ms1);

                                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };
                

                return Ok(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }

        //Teste
        [HttpGet("GerarRelatorioTeste3")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioZ()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                /*

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.teste3));


                Person t1 = new() { Name = "teste 11", Surname = "só x teste", Id = 11, Age = 1 };
                Person t2 = new() { Name = "teste 22", Surname = "só y teste", Id = 22, Age = 2 };
                Person t3 = new() { Name = "teste 33", Surname = "só z teste", Id = 33, Age = 3 };

                List<Person> ty = new();
                ty.Add(t1);
                ty.Add(t2);
                ty.Add(t3);

                List<object> tz = new();

                tz.Add(new { Name = "teste 11", Surname = "só x teste", Id = 11, Age = 1 });
                tz.Add(new { Name = "teste 99", Surname = "só x teste", Id = 11, Age = 9 });

                Type entityType = typeof(Person);
                DataTable table = new DataTable(entityType.Name);
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
                foreach (PropertyDescriptor prop in properties)
                {
                    table.Columns.Add(prop.Name, prop.PropertyType);
                }


                foreach (Person item in ty)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item);
                    }
                    table.Rows.Add(row);
                }


                relatorio1.Report.RegisterData(tz, "Persons");
                


                ((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Persons");
                relatorio1.Report.GetDataSource("Persons").Enabled = true;

                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };
                
                return Ok(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }

        //Balancete
        [HttpGet("GerarRelatorioTeste4")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioW()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();

                ///Executar 4 vezes se for trimestral, 2x se for semestral
                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.Balancete, new[] { "1", "2020-01-01", "2020-12-31" });

                DbDataReader dados = cmd.ExecuteReader();

                //var yx = dados.OfType<List<ObjetoSqlBalancante>>().ToList();

                DataTable datatable = new();
                datatable.Load(dados);

                var a = datatable.AsEnumerable().ToList<DataRow>().Select(x => new ObjetoSqlBalancante()
                {
                    Debito = x.ItemArray.ElementAt(0) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(0) : "0",
                    Credito = x.ItemArray.ElementAt(1) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(1) : "0",
                    Total = (double)x.ItemArray.ElementAt(2),
                    PlanoContabilId = (int)x.ItemArray.Last(),
                    TipoTotalInicial = x.ItemArray.ElementAt(0) is DBNull ? 'C' : 'D'
                }).ToList();

                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.PlanoContabil, new string[] { "1" });

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var b = datatable.AsEnumerable().ToList<DataRow>().Select(x => new PlanoContabilConta()
                {
                    CodigoCompleto = (string)x.ItemArray.ElementAt(0),
                    CodigoReduzido = (int)x.ItemArray.ElementAt(1),
                    Nome = (string)x.ItemArray.ElementAt(2),
                    Tipo = ((string)x.ItemArray.ElementAt(3))[0],
                }).ToList();

                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.AuxRelatorioSaldoPeriodico, new[] { "1", "2020-01-01", "2020-12-31" });

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var c = datatable.AsEnumerable().ToList<DataRow>().Select(x => new SaldoPeriodicoContabilTela()
                {
                    PlanoContabilContaCodigo = (string)x.ItemArray.ElementAt(4),
                    Saldo = (double)x.ItemArray.ElementAt(9),
                    Tipo = ((string)x.ItemArray.ElementAt(8))[0],
                }).ToList();

                var z = b.Select(x => new RelatorioBalancete()
                {
                    Conta = x.CodigoCompleto,
                    Nome = x.Nome,
                    SaldoAnterior = Math.Round(Math.Abs(calcularSaldoInicial(x.CodigoCompleto, a)), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAnterior = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(calcularSaldoInicial(x.CodigoCompleto, a))),
                    SaldoDebito = Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    SaldoCredito = Math.Round(a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    SaldoAtual = Math.Round(Math.Abs(c.Where(y => y.PlanoContabilContaCodigo.StartsWith(x.CodigoCompleto)).Sum(y => utilContabilidade.ConverterValor(x.Tipo, y.Tipo, y.Saldo))), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAtual = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(c.Where(y => y.PlanoContabilContaCodigo.StartsWith(x.CodigoCompleto)).Sum(y => utilContabilidade.ConverterValor(x.Tipo, y.Tipo, y.Saldo))))
                }).ToList();

                var zz = z.Where(x => x.Conta.Count() == 1).ToList();

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.Balancete));

                relatorio1.Report.SetParameterValue("TemAssinatura", true);
                relatorio1.Report.SetParameterValue("TemResumoGruposPrincipais", true);
                relatorio1.Report.SetParameterValue("Empresa", "Empresa teste LTDA - 00.000.000/0000-01" + Environment.NewLine + "NIRE: 12345678 / Data 01/01/2022");
                relatorio1.Report.SetParameterValue("Periodo", "01/01/2020 a 31/12/2020");
                relatorio1.Report.SetParameterValue("Responsavel", "Teste" + Environment.NewLine + "Teste1" + Environment.NewLine + "Teste2" + Environment.NewLine + "Teste3");
                relatorio1.Report.SetParameterValue("Contador", "Teste" + Environment.NewLine + "Teste1" + Environment.NewLine + "Teste2" + Environment.NewLine + "Teste3");
                relatorio1.Report.SetParameterValue("Resultado", 0.00);
                relatorio1.Report.SetParameterValue("ResultadoTipo", 'D');


                relatorio1.Report.RegisterData(z, "Tabela");
                relatorio1.Report.RegisterData(zz, "Tabela2");

                //((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Tabela");
                //relatorio1.Report.GetDataSource("Tabela").Enabled = true;



                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };

                return Ok(rel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }


        //LivroRazao
        [HttpGet("GerarRelatorioTeste5")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioQ()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();


                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.LivroDiario, new[] { "1", "2020-01-01", "2020-12-31" });

                DbDataReader dados = cmd.ExecuteReader();

                //var yx = dados.OfType<List<ObjetoSqlBalancante>>().ToList();

                DataTable datatable = new();
                datatable.Load(dados);

                var a = datatable.AsEnumerable().ToList<DataRow>().Select(x => new ObjetoSqlLivroRazao()
                {
                    Data = (DateTime)x.ItemArray.ElementAt(0),
                    Debito = x.ItemArray.ElementAt(1) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(1) : "0",
                    DebitoNome = x.ItemArray.ElementAt(2) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(2) : "",
                    Credito = x.ItemArray.ElementAt(3) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(3) : "0",
                    CreditoNome = x.ItemArray.ElementAt(4) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(4) : "",
                    NumeroLancamento = (int)x.ItemArray.ElementAt(5),
                    Valor = (double)x.ItemArray.ElementAt(6),
                    Historico = (string)x.ItemArray.ElementAt(7),
                }).ToList();

                var bb = a.Where(x => x.Debito.Contains("1.1.1.01.0001")).ToList().Select(x => new RelatorioLivroRazao()
                {
                    GrupoNome = "Conta Caixa",
                    GrupoConta = "1.1.1.01.0001",
                    NumeroLancamento = x.NumeroLancamento,
                    Data = x.Data,
                    ContaContraPartida = x.Credito,
                    SaldoDebito = x.Valor,
                    SaldoCredito = null,
                    Historico = x.Historico
                }).ToList();

                bb.AddRange(a.Where(x => x.Credito.Contains("1.1.1.01.0001")).ToList().Select(x => new RelatorioLivroRazao()
                {
                    GrupoNome = "Conta Caixa",
                    GrupoConta = "1.1.1.01.0001",
                    NumeroLancamento = x.NumeroLancamento,
                    Data = x.Data,
                    ContaContraPartida = x.Debito,
                    SaldoCredito = x.Valor,
                    SaldoDebito = null,
                    Historico = x.Historico
                }));

                var xyzw = a.Where(x => (x.Debito.Contains("1.1.1.01.0001") && x.Credito.Contains("0")) || (x.Debito.Contains("0") && x.Debito.Contains("1.1.1.01.0001"))).ToList();

                //bb.RemoveRange(xyzw);

                double saldo = xyzw.First().Valor;
                char tipoSaldo = 'D';

                bb.RemoveAt(0);

                var bbb = bb.OrderBy(x => x.Data).ThenBy(x => x.NumeroLancamento).ToList().Select(x => new RelatorioLivroRazao()
                {
                    GrupoNome = "Conta Caixa",
                    GrupoConta = "1.1.1.01.0001",
                    NumeroLancamento = x.NumeroLancamento,
                    Data = x.Data,
                    ContaContraPartida = x.ContaContraPartida,
                    SaldoCredito = x.SaldoCredito is not null ? x.SaldoCredito : null,
                    SaldoDebito = x.SaldoDebito is not null ? x.SaldoDebito : null,
                    Historico = x.Historico,
                }).ToList();

                bbb.ForEach(x =>
                {
                    x.SaldoTotal = Math.Abs(x.SaldoCredito is null ? (double)(saldo + x.SaldoDebito) : (double)(saldo - x.SaldoCredito));
                    x.Total = x.SaldoCredito is null ? (double)(saldo + x.SaldoDebito) : (double)(saldo - x.SaldoCredito);
                    x.TipoSaldoTotal = double.IsNegative(x.SaldoCredito is null ? (double)(saldo + x.SaldoDebito) : (double)(saldo - x.SaldoCredito)) ? 'C' : 'D';
                    saldo = x.Total;
                    tipoSaldo = x.TipoSaldoTotal;
                });

                //Geral
                List<RelatorioLivroRazaoGrupoTotalizador> ddd = new()
                {
                    new()
                    {
                        GrupoSaldoInicial = xyzw.First().Valor,
                        GrupoTipoSaldoInicial = xyzw.First().Debito.Contains("1.1.1.01.0001") ? 'D': 'C',
                        GrupoSaldoAtual = bbb.Last().SaldoTotal,
                        GrupoTipoSaldoAtual = bbb.Last().TipoSaldoTotal,
                        GrupoSaldoDebito = bbb.Sum(x => x.SaldoDebito),
                        GrupoSaldoCredito = bbb.Sum(x => x.SaldoCredito),
                    }
                };



                //Mensal
                ddd.Clear();
                for (int i = 1; i < 13; i++)
                {
                    ddd.Add(new()
                    {
                        GrupoSaldoInicial = xyzw.First().Valor,
                        GrupoTipoSaldoInicial = xyzw.First().Debito.Contains("1.1.1.01.0001") ? 'D' : 'C',
                        GrupoSaldoAtual = bbb.Where(x => x.Data.Month == i).Last().SaldoTotal,
                        GrupoTipoSaldoAtual = bbb.Where(x => x.Data.Month == i).Last().TipoSaldoTotal,
                        GrupoSaldoDebito = bbb.Where(x => x.Data.Month == i).Sum(x => x.SaldoDebito),
                        GrupoSaldoCredito = bbb.Where(x => x.Data.Month == i).Sum(x => x.SaldoCredito),
                    }
                );
                }


                //Diario
                ddd.Clear();
                for (int i = 0; i < bbb.Select(x => x.Data).Distinct().Count(); i++)
                {
                    var datea = bbb.Select(x => x.Data).Distinct().ToList()[i];

                    ddd.Add(new()
                    {
                        GrupoSaldoInicial = xyzw.First().Valor,
                        GrupoTipoSaldoInicial = xyzw.First().Debito.Contains("1.1.1.01.0001") ? 'D' : 'C',
                        GrupoSaldoAtual = bbb.Where(x => x.Data == datea).Last().SaldoTotal,
                        GrupoTipoSaldoAtual = bbb.Where(x => x.Data == datea).Last().TipoSaldoTotal,
                        GrupoSaldoDebito = bbb.Where(x => x.Data == datea).Sum(x => x.SaldoDebito),
                        GrupoSaldoCredito = bbb.Where(x => x.Data == datea).Sum(x => x.SaldoCredito),
                    }
                );
                }


                var x = 1;

                /*
                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.planoContabil, "1");

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var b = datatable.AsEnumerable().ToList<DataRow>().Select(x => new PlanoContabilConta()
                {
                    CodigoCompleto = (string)x.ItemArray.ElementAt(0),
                    CodigoReduzido = (int)x.ItemArray.ElementAt(1),
                    Nome = (string)x.ItemArray.ElementAt(2),
                    Tipo = ((string)x.ItemArray.ElementAt(3))[0],
                }).ToList();

                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.auxRelatorioSaldoPeriodico, new[] { "1", "2020-01-01", "2020-12-31" });

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var c = datatable.AsEnumerable().ToList<DataRow>().Select(x => new SaldoPeriodicoContabilTela()
                {
                    PlanoContabilContaCodigo = (string)x.ItemArray.ElementAt(0),
                    Saldo = (double)x.ItemArray.ElementAt(1),
                    Tipo = ((string)x.ItemArray.ElementAt(2))[0],
                }).ToList();

                var z = b.Select(x => new RelatorioBalancete()
                {
                    Conta = x.CodigoCompleto,
                    Nome = x.Nome,
                    SaldoAnterior = Math.Round(Math.Abs(calcularSaldoInicial(x.CodigoCompleto, a)), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAnterior = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(calcularSaldoInicial(x.CodigoCompleto, a))),
                    SaldoDebito = Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    SaldoCredito = Math.Round(a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    SaldoAtual = Math.Round(Math.Abs(c.Where(y => y.PlanoContabilContaCodigo.StartsWith(x.CodigoCompleto)).Sum(y => utilContabilidade.ConverterValor(x.Tipo.ToString(), y.Tipo.ToString(), y.Saldo))), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAtual = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(c.Where(y => y.PlanoContabilContaCodigo.StartsWith(x.CodigoCompleto)).Sum(y => utilContabilidade.ConverterValor(x.Tipo.ToString(), y.Tipo.ToString(), y.Saldo))))
                }).ToList();

                var zz = z.Where(x => x.Conta.Count() == 1).ToList();

                


                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.LivroRazao));

                //relatorio1.Report.SetParameterValue("TemAssinatura", true);
                //relatorio1.Report.SetParameterValue("TemResumoGruposPrincipais", true);
                relatorio1.Report.SetParameterValue("Empresa", "Empresa teste LTDA - 00.000.000/0000-01" + Environment.NewLine + "NIRE: 12345678 / Data 01/01/2022");
                relatorio1.Report.SetParameterValue("Periodo", "01/01/2020 a 31/12/2020");
                //relatorio1.Report.SetParameterValue("TemTotalizadorMensalDiario", true);
                relatorio1.Report.SetParameterValue("TipoTotalizador", "Dia");
                //relatorio1.Report.SetParameterValue("Responsavel", "Teste" + Environment.NewLine + "Teste1" + Environment.NewLine + "Teste2" + Environment.NewLine + "Teste3");
                //relatorio1.Report.SetParameterValue("Contador", "Teste" + Environment.NewLine + "Teste1" + Environment.NewLine + "Teste2" + Environment.NewLine + "Teste3");
                //relatorio1.Report.SetParameterValue("Resultado", 0.00);
                //relatorio1.Report.SetParameterValue("ResultadoTipo", 'D');


                //relatorio1.Report.RegisterData(bbb, "Tabela");
                //relatorio1.Report.RegisterData(ddd, "Grupo");

                //((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Tabela");
                //relatorio1.Report.GetDataSource("Tabela").Enabled = true;



                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };

                return Ok(rel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }

        //LivroCaixa
        [HttpGet("GerarRelatorioTeste6")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioA()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();


                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.LivroDiario, new[] { "1", "2020-01-01", "2020-12-31" });

                DbDataReader dados = cmd.ExecuteReader();

                //var yx = dados.OfType<List<ObjetoSqlBalancante>>().ToList();

                DataTable datatable = new();
                datatable.Load(dados);

                var a = datatable.AsEnumerable().ToList<DataRow>().Select(x => new ObjetoSqlLivroRazao()
                {
                    Data = (DateTime)x.ItemArray.ElementAt(0),
                    Debito = x.ItemArray.ElementAt(1) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(1) : "0",
                    DebitoNome = x.ItemArray.ElementAt(2) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(2) : "",
                    Credito = x.ItemArray.ElementAt(3) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(3) : "0",
                    CreditoNome = x.ItemArray.ElementAt(4) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(4) : "",
                    NumeroLancamento = (int)x.ItemArray.ElementAt(5),
                    Valor = (double)x.ItemArray.ElementAt(6),
                    Historico = (string)x.ItemArray.ElementAt(7),
                }).ToList();

                var bb = a.Where(x => x.Debito.Contains("1.1.1.01.0001")).ToList().Select(x => new RelatorioLivroRazao()
                {
                    GrupoNome = "Conta Caixa",
                    GrupoConta = "1.1.1.01.0001",
                    NumeroLancamento = x.NumeroLancamento,
                    Data = x.Data,
                    ContaContraPartida = x.Credito,
                    SaldoDebito = x.Valor * -1,
                    SaldoCredito = null,
                    Historico = x.Historico
                }).ToList();

                bb.AddRange(a.Where(x => x.Credito.Contains("1.1.1.01.0001")).ToList().Select(x => new RelatorioLivroRazao()
                {
                    GrupoNome = "Conta Caixa",
                    GrupoConta = "1.1.1.01.0001",
                    NumeroLancamento = x.NumeroLancamento,
                    Data = x.Data,
                    ContaContraPartida = x.Debito,
                    SaldoCredito = x.Valor,
                    SaldoDebito = null,
                    Historico = x.Historico
                }));

                var xyzw = a.Where(x => (x.Debito.Contains("1.1.1.01.0001") && x.Credito.Contains("0")) || (x.Debito.Contains("0") && x.Debito.Contains("1.1.1.01.0001"))).ToList();

                //bb.RemoveRange(xyzw);

                double saldo = 0;
                char tipoSaldo = 'D';

                var bbb = bb.OrderBy(x => x.Data).ThenBy(x => x.NumeroLancamento).ToList().Select(x => new RelatorioLivroRazao()
                {
                    GrupoNome = "Conta Caixa",
                    GrupoConta = "1.1.1.01.0001",
                    NumeroLancamento = x.NumeroLancamento,
                    Data = x.Data,
                    ContaContraPartida = x.ContaContraPartida,
                    SaldoCredito = x.SaldoCredito is not null ? x.SaldoCredito : null,
                    SaldoDebito = x.SaldoDebito is not null ? x.SaldoDebito : null,
                    Historico = x.Historico,
                }).ToList();

                bbb.ForEach(x =>
                {
                    x.SaldoTotal = (x.SaldoCredito is null ? (double)(saldo + x.SaldoDebito) : (double)(saldo + x.SaldoCredito));
                    x.Total = x.SaldoCredito is null ? (double)(saldo + x.SaldoDebito) : (double)(saldo + x.SaldoCredito);
                    x.TipoSaldoTotal = double.IsNegative(x.SaldoCredito is null ? (double)(saldo + x.SaldoDebito) : (double)(saldo + x.SaldoCredito)) ? 'C' : 'D';
                    saldo = x.Total;
                    tipoSaldo = x.TipoSaldoTotal;
                });

                //Geral
                List<RelatorioLivroRazaoGrupoTotalizador> ddd = new()
                {
                    new()
                    {
                        GrupoSaldoInicial = xyzw.First().Valor,
                        GrupoTipoSaldoInicial = xyzw.First().Debito.Contains("1.1.1.01.0001") ? 'D': 'C',
                        GrupoSaldoAtual = bbb.Last().SaldoTotal,
                        GrupoTipoSaldoAtual = bbb.Last().TipoSaldoTotal,
                        GrupoSaldoDebito = bbb.Sum(x => x.SaldoDebito),
                        GrupoSaldoCredito = bbb.Sum(x => x.SaldoCredito),
                    }
                };


                /*
                //Mensal
                ddd.Clear();
                for (int i = 1; i < 13; i++)
                {
                    ddd.Add(new()
                    {
                        GrupoSaldoInicial = xyzw.First().Valor,
                        GrupoTipoSaldoInicial = xyzw.First().Debito.Contains("1.1.1.01.0001") ? 'D' : 'C',
                        GrupoSaldoAtual = bbb.Where(x => x.Data.Month == i).Last().SaldoTotal,
                        GrupoTipoSaldoAtual = bbb.Where(x => x.Data.Month == i).Last().TipoSaldoTotal,
                        GrupoSaldoDebito = bbb.Where(x => x.Data.Month == i).Sum(x => x.SaldoDebito),
                        GrupoSaldoCredito = bbb.Where(x => x.Data.Month == i).Sum(x => x.SaldoCredito),
                    }
                );
                }
                





                var x = 1;

                /*
                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.planoContabil, "1");

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var b = datatable.AsEnumerable().ToList<DataRow>().Select(x => new PlanoContabilConta()
                {
                    CodigoCompleto = (string)x.ItemArray.ElementAt(0),
                    CodigoReduzido = (int)x.ItemArray.ElementAt(1),
                    Nome = (string)x.ItemArray.ElementAt(2),
                    Tipo = ((string)x.ItemArray.ElementAt(3))[0],
                }).ToList();

                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.auxRelatorioSaldoPeriodico, new[] { "1", "2020-01-01", "2020-12-31" });

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var c = datatable.AsEnumerable().ToList<DataRow>().Select(x => new SaldoPeriodicoContabilTela()
                {
                    PlanoContabilContaCodigo = (string)x.ItemArray.ElementAt(0),
                    Saldo = (double)x.ItemArray.ElementAt(1),
                    Tipo = ((string)x.ItemArray.ElementAt(2))[0],
                }).ToList();

                var z = b.Select(x => new RelatorioBalancete()
                {
                    Conta = x.CodigoCompleto,
                    Nome = x.Nome,
                    SaldoAnterior = Math.Round(Math.Abs(calcularSaldoInicial(x.CodigoCompleto, a)), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAnterior = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(calcularSaldoInicial(x.CodigoCompleto, a))),
                    SaldoDebito = Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    SaldoCredito = Math.Round(a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    SaldoAtual = Math.Round(Math.Abs(c.Where(y => y.PlanoContabilContaCodigo.StartsWith(x.CodigoCompleto)).Sum(y => utilContabilidade.ConverterValor(x.Tipo.ToString(), y.Tipo.ToString(), y.Saldo))), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAtual = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(c.Where(y => y.PlanoContabilContaCodigo.StartsWith(x.CodigoCompleto)).Sum(y => utilContabilidade.ConverterValor(x.Tipo.ToString(), y.Tipo.ToString(), y.Saldo))))
                }).ToList();

                var zz = z.Where(x => x.Conta.Count() == 1).ToList();

                


                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.LivroCaixa));

                //relatorio1.Report.SetParameterValue("TemAssinatura", true);
                //relatorio1.Report.SetParameterValue("TemResumoGruposPrincipais", true);
                relatorio1.Report.SetParameterValue("Empresa", "Empresa teste LTDA - 00.000.000/0000-01" + Environment.NewLine + "NIRE: 12345678 / Data 01/01/2022");
                relatorio1.Report.SetParameterValue("Periodo", "01/01/2020 a 31/12/2020");
                //relatorio1.Report.SetParameterValue("TemTotalizadorMensalDiario", true);
                //relatorio1.Report.SetParameterValue("TipoTotalizador", "Dia");
                //relatorio1.Report.SetParameterValue("Responsavel", "Teste" + Environment.NewLine + "Teste1" + Environment.NewLine + "Teste2" + Environment.NewLine + "Teste3");
                //relatorio1.Report.SetParameterValue("Contador", "Teste" + Environment.NewLine + "Teste1" + Environment.NewLine + "Teste2" + Environment.NewLine + "Teste3");
                //relatorio1.Report.SetParameterValue("Resultado", 0.00);
                //relatorio1.Report.SetParameterValue("ResultadoTipo", 'D');


                relatorio1.Report.RegisterData(bbb, "Tabela");
                relatorio1.Report.RegisterData(ddd, "Grupo");

                //((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Tabela");
                //relatorio1.Report.GetDataSource("Tabela").Enabled = true;



                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };

                return Ok(rel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }


        //Balanco
        [HttpGet("GerarRelatorioTeste7")]
        public async Task<ActionResult<ObjetoRelatorio>> GerarRelatorioB()
        {
            try
            {
#if !DEBUG
                    conexao = new() { ConnectionString = @"Host=ESTU00; Port=23286; Database=oriente; Username=oriente; Password=sistemas; Pooling=True" };
#endif

                conexao.Open();

                ///Executar 4 vezes se for trimestral, 2x se for semestral
                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.Balancete, new[] { "1", "2020-01-01", "2020-12-31" });

                DbDataReader dados = cmd.ExecuteReader();

                //var yx = dados.OfType<List<ObjetoSqlBalancante>>().ToList();

                DataTable datatable = new();
                datatable.Load(dados);

                var a = datatable.AsEnumerable().ToList<DataRow>().Select(x => new ObjetoSqlBalancante()
                {
                    Debito = x.ItemArray.ElementAt(0) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(0) : "0",
                    Credito = x.ItemArray.ElementAt(1) is not DBNull ? (string)x.ItemArray.ElementAtOrDefault(1) : "0",
                    Total = (double)x.ItemArray.ElementAt(2),
                    PlanoContabilId = (int)x.ItemArray.Last(),
                    TipoTotalInicial = x.ItemArray.ElementAt(0) is DBNull ? 'C' : 'D'
                }).ToList();

                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.PlanoContabil, new string[] { "1" });

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);

                var b = datatable.AsEnumerable().ToList<DataRow>().Select(x => new PlanoContabilConta()
                {
                    CodigoCompleto = (string)x.ItemArray.ElementAt(0),
                    CodigoReduzido = (int)x.ItemArray.ElementAt(1),
                    Nome = (string)x.ItemArray.ElementAt(2),
                    Tipo = ((string)x.ItemArray.ElementAt(3))[0],
                    NaturezaSped = (short)x.ItemArray.ElementAt(4),
                }).Where(x => x.NaturezaSped == 1 || x.NaturezaSped == 2).ToList();

                cmd.CommandText = utilContabilidade.ComandoSqlRelatorio(tipoRelatorio.AuxRelatorioSaldoPeriodico, new[] { "1", "2020-01-01", "2020-12-31" });

                dados = cmd.ExecuteReader();

                datatable = new();
                datatable.Load(dados);


                double total = 0;
                char tipoTotal = ' ';

                var z = b.Select(x => new RelatorioBalanco()
                {
                    ContaBase = b.FirstOrDefault(y => y.Nivel == 2 && x.CodigoCompleto.StartsWith(y.CodigoCompleto), new() { CodigoCompleto = "0" }).CodigoCompleto,
                    NomeBase = b.FirstOrDefault(y => y.Nivel == 2 && x.CodigoCompleto.StartsWith(y.CodigoCompleto), new() { Nome = "Vazio" }).Nome,
                    Conta = x.CodigoCompleto,
                    Nome = x.Nome.PadLeftAbsoluto(x.Nivel*4),
                    SaldoAtual = Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total) -
                                        a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero),
                    TipoSaldoAtual = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total) -
                                        a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero))),
                    SaldoTotalAtual = x.Nivel == 2 ? total = Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total) -
                                        a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero) : total,
                    TipoSaldoTotalAtual = x.Nivel == 2 ? tipoTotal = utilContabilidade.ConverterTipoConta(x.Tipo, double.IsNegative(Math.Round(a.Where(y => y.Debito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total) -
                                        a.Where(y => y.Credito.StartsWith(x.CodigoCompleto)).Sum(y => y.Total), 2, MidpointRounding.AwayFromZero))) : tipoTotal,

                    Nivel = x.Nivel
                }).ToList().Where(x => x.SaldoAtual != 0).ToList();

                short nivelMaximo = z.Max(x => x.Nivel);
                short nivelAnterior = 0;


                RelatorioBalanco objeto = null;

                /*
                for (int i = 0; i < z.Count; i++)
                {
                    var abc = z[i];

                    if (z[i].Nivel == 1) { continue; }
                    else if (z[i].Nivel == 2 && objeto is null) { objeto = new() { Nome = $"Total {z[i].Nome}".PadLeftAbsoluto(z[i].Nivel*4), SaldoAtual = z[i].SaldoAtual, TipoSaldoAtual = z[i].TipoSaldoAtual, Conta = z[i].Conta, Nivel = z[i].Nivel }; z[i].SaldoAtual = 0; z[i].TipoSaldoAtual = ' '; z[i].Nome = z[i].Nome.PadLeftAbsoluto(z[i].Nivel*4); }
                    else if (z[i].Nivel == 2 && objeto is not null) { z.Insert(i, objeto);  i++; objeto = new() { Nome = $"Total {z[i].Nome}".PadLeftAbsoluto(z[i].Nivel*4), SaldoAtual = z[i].SaldoAtual, TipoSaldoAtual = z[i].TipoSaldoAtual, Conta = z[i].Conta, Nivel = z[i].Nivel }; z[i].SaldoAtual = 0; z[i].TipoSaldoAtual = ' '; z[i].Nome = z[i].Nome.PadLeftAbsoluto(z[i].Nivel*4); }
                    else if (z[i].Nivel != 2 && z[i].Nivel != nivelMaximo) { z[i].Nome = z[i].Nome.PadLeftAbsoluto(z[i].Nivel*4); }
                    else { z[i].Nome = z[i].Nome.PadLeftAbsoluto(z[i].Nivel*4); }
                }
                

                var totalizadores = z.Where(x => x.Nivel == 1).ToList();

                double totalAtivo = totalizadores[0].SaldoAtual;
                double totalPassivo = totalizadores[1].SaldoAtual;

                z.RemoveAll(x => x.Nivel == 1);
                z.RemoveAll(x => x.Nivel == 2);

                //z.Where(x => x.Conta.ContarNivel() == 2).ToList().ForEach(x => x.ContaPai = x.Conta);

                //var zativo = z.Where(x => x.Conta.StartsWith("1")).ToList();

                //var zativo2 = z.GroupBy(x => x.Conta).ToList();

                //var zz = z.Where(x => x.Conta.Count() == 1).Select(x => new GrupoSqlLivroDiario()
                //{
                //    GrupoNome = x.Conta,
                //    GrupoSaldoAtual = x.SaldoAtual,
                //    GrupoTipoSaldoAtual = x.TipoSaldoAtual,
                //}).ToList();

                var zt = z.Where(x => x.Conta.StartsWith("1")).ToList();

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.Balanco));


                relatorio1.Report.SetParameterValue("Empresa", "Empresa teste LTDA - 00.000.000/0000-01" + Environment.NewLine + "NIRE: 12345678 / Data 01/01/2022");
                relatorio1.Report.SetParameterValue("Periodo", "01/01/2020 a 31/12/2020");

                relatorio1.Report.SetParameterValue("TotalAtivo", totalAtivo);
                relatorio1.Report.SetParameterValue("TotalPassivo", totalPassivo);
                relatorio1.Report.SetParameterValue("ValorExtenso", totalAtivo.ValorPorExtenso());

                relatorio1.Report.SetParameterValue("CidadeData", "Campo Grande, 15 de agosto 2020");
                relatorio1.Report.SetParameterValue("Responsavel", "Teste modelo" + Environment.NewLine + "Teste modelo" + Environment.NewLine + "RG: 1234567" + Environment.NewLine + "CPF: 123.456.789-99");
                relatorio1.Report.SetParameterValue("Contador", "Contador modelo" + Environment.NewLine + "CRC: 1MS234567" + Environment.NewLine + "CPF: 123.456.789-99");


                relatorio1.Report.RegisterData(z.Where(x => x.Conta.StartsWith("1")).ToList(), "Tabela");
                relatorio1.Report.RegisterData(z.Where(x => x.Conta.StartsWith("2")).ToList(), "Tabela2");

                //((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("Tabela");
                //relatorio1.Report.GetDataSource("Tabela").Enabled = true;



                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                string texto = GerarHtml(relatorio1);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;

                conversor.Options.MarginLeft = 20;
                conversor.Options.MarginTop = 20;
                conversor.Options.MarginRight = 20;
                conversor.Options.MarginBottom = 20;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);

                pdf.Save(ms1);

                ObjetoRelatorio rel = new() { PaginaHtml = texto, ArquivoPDF = ms1.ToArray() };

                return Ok(rel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return BadRequest(conexao.ConnectionString);
            }
        }






        [HttpGet("GerarRelatorio/PDF")]
        public async Task<IActionResult> GerarRelatorioPdf()
        {
            try
            {
                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = "SELECT * FROM oriente.contabilidade.planocontabil";

                DbDataReader dados = cmd.ExecuteReader();
                PlanoContabil y = new();

                //var z = JsonSerializer.Serialize(dados);

                //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                DataTable datatable = new();
                datatable.Load(dados);

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.teste));
                relatorio1.Report.RegisterData(datatable, "contabilidade_planocontabil");

                ((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("contabilidade_planocontabil");
                relatorio1.Report.GetDataSource("contabilidade_planocontabil").Enabled = true;

                relatorio1.Report.Prepare();

                var document = new PdfSharpCore.Pdf.PdfDocument();



                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("OpenSans", 20, XFontStyle.Bold);

                gfx.DrawString(
                    "Hello World!", font, XBrushes.Black,
                    new XRect(20, 20, page.Width, page.Height),
                    XStringFormats.Center);



                using (MemoryStream ms = new MemoryStream())
                {
                    document.Save(ms);

                    return File(ms.ToArray(), "application/text");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }



        [HttpGet("GerarRelatorio/XLSX")]
        public async Task<IActionResult> GerarRelatorioXLSX()
        {
            try
            {
                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = "SELECT * FROM oriente.contabilidade.planocontabil";

                DbDataReader dados = cmd.ExecuteReader();
                PlanoContabil y = new();

                //var z = JsonSerializer.Serialize(dados);

                //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                DataTable datatable = new();
                datatable.Load(dados);

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.teste));
                relatorio1.Report.RegisterData(datatable, "contabilidade_planocontabil");

                ((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("contabilidade_planocontabil");
                relatorio1.Report.GetDataSource("contabilidade_planocontabil").Enabled = true;

                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.ArchB;
                //conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                //conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.AutoFit;
                conversor.Options.MarginTop = 10;
                conversor.Options.MarginBottom = 7;
                conversor.Options.MarginLeft = 10;
                conversor.Options.MarginRight = 7;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.AutoFit;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);


                //pdf.CompressionLevel = PdfCompressionLevel.Best;


                pdf.Save(ms1);

                return File(ms1.ToArray(), "application/text");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }



        [HttpGet("GerarRelatorio/DOCX")]
        public async Task<IActionResult> GerarRelatorioDOCX()
        {
            try
            {
                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = "SELECT * FROM oriente.contabilidade.planoreferencial as pr WHERE pr.Tipo = 1 limit 200";

                DbDataReader dados = cmd.ExecuteReader();
                PlanoContabil y = new();

                //var z = JsonSerializer.Serialize(dados);

                //var a = utilContabilidade.SerializarObjeto(dados, y).ToString();

                DataTable datatable = new();
                datatable.Load(dados);

                WebReport relatorio1 = new WebReport();
                relatorio1.Report.Load(new MemoryStream(Properties.Resources.testex));
                relatorio1.Report.RegisterData(datatable, "contabilidade_planoreferencial");

                ((DataBand)relatorio1.Report.FindObject("Data1")).DataSource = relatorio1.Report.GetDataSource("contabilidade_planoreferencial");
                relatorio1.Report.GetDataSource("contabilidade_planoreferencial").Enabled = true;

                relatorio1.Report.Prepare();

                MemoryStream ms = new();
                MemoryStream ms1 = new();

                HTMLExport html = new HTMLExport();
                // We need embedded pictures inside html
                html.EmbedPictures = true;
                // Enable all report pages in one html file
                html.SinglePage = false;
                // We don't need a subfolder for pictures and additional files
                html.SubFolder = false;
                // Enable layered HTML
                html.Layers = true;
                // Turn off the toolbar with navigation
                html.Navigator = false;
                // Save the report in html
                relatorio1.Report.Export(html, ms);

                string texto = Encoding.UTF8.GetString(ms.ToArray());

                texto = consertarHtml(texto);



                HtmlToPdf conversor = new();
                conversor.Options.PdfStandard = PdfStandard.PdfA;
                conversor.Options.PdfPageSize = PdfPageSize.A4;
                //conversor.Options.PdfPageCustomSize = new(595, 842);
                conversor.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;
                conversor.Options.AutoFitWidth = HtmlToPdfPageFitMode.NoAdjustment;
                //conversor.Options.MarginTop = 10;
                //conversor.Options.MarginBottom = 5;
                //conversor.Options.MarginLeft = 10;
                //conversor.Options.MarginRight = 5;
                //conversor.Options.WebPageFixedSize = true;

                SelectPdf.PdfDocument pdf = conversor.ConvertHtmlString(texto);


                //pdf.CompressionLevel = PdfCompressionLevel.Best;


                pdf.Save(ms1);

                return File(ms1.ToArray(), "application/text");

                //return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }



        [HttpGet("{Comando}/Get")]
        public async Task<string> GetZ(string comando)
        {
            await Task.Delay(0);

            try
            {
                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = comando;

                DbDataReader dados = cmd.ExecuteReader();

                DataTable tabela = new();
                tabela.Load(dados);

                //string JSONString = JsonConvert.SerializeObject(dt);

                return null;//JsonConvert.SerializeObject(tabela);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }

        [HttpGet("{Comando}/GetX")]
        public async Task<HashSet<Pessoa>> GetX(string comando)
        {
            await Task.Delay(0);

            try
            {
                conexao.Open();

                DbCommand cmd = conexao.CreateCommand();
                cmd.CommandText = comando;

                var a = cmd.ExecuteReader();

                DataTable dt = new();
                dt.Load(a);

                string JSONString = string.Empty;
                //JSONString = JsonConvert.SerializeObject(dt);

                //var x = JsonConvert.DeserializeObject<HashSet<Pessoa>>(JSONString);

                //return x;

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }

        [HttpGet("{Comando}/GetY")]
        public async Task<HashSet<Pessoa>> GetY(string comando, [FromBody] Type tipo)
        {
            await Task.Delay(0);

            try
            {
                NpgsqlConnection conn = new() { ConnectionString = @"Host = 192.168.137.100; Port = 23286; Database = oriente; Username = oriente; Password = sistemas; Pooling = true" };
                conn.Open();

                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = comando;

                var a = cmd.ExecuteReader();

                DataTable dt = new();
                dt.Load(a);

                string JSONString = string.Empty;
                //JSONString = JsonConvert.SerializeObject(dt);

                //var x = JsonConvert.DeserializeObject<HashSet<Pessoa>>(JSONString);

                //return x;

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Encontrado: [DBController] Get {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                return null;
            }
        }

        #endregion
    }
}

*/
