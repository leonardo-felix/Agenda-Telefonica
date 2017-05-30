using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace AgendaTelefonica
{
    public class Pessoa : Label
    {
        public string nome;
        public string[] telefones;
    }

    public class GrupoRaiz : TreeViewItem
    {
        public string Nome;
        public const string tag = "lista_telefonica";

        public string nome
        {
            get
            {
                return this.Nome;
            }
            set
            {
                this.Nome = value;
                this.Header = value;
            }
        }

        public virtual XElement paraXML()
        {
            XElement elemento = new XElement(tag);

            elemento.SetAttributeValue("nome", this.nome);

            return elemento;
        }

        public static GrupoRaiz gerar(string nome, string id=null)
        {
            GrupoRaiz grp = new GrupoRaiz();
            grp.nome = nome;

            return grp;
        }
    }

    public class Grupo : GrupoRaiz
    {
        public string responsavel;
        public new const string tag = "grupo";
        public string _id = null;

        public string id
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._id))
                {
                    this._id = generateID();
                }
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        public static Grupo gerar(string nome, string responsavel="", string id=null)
        {
            Grupo grp = new Grupo();
            grp.nome = nome;
            grp.responsavel = responsavel;

            if (!string.IsNullOrEmpty(id))
            {
                grp.id = id;
            }

            return grp;
        }

        public new XElement paraXML()
        {
            XElement elemento = new XElement(tag);

            elemento.SetAttributeValue("nome", this.nome);
            elemento.SetAttributeValue("responsavel", this.responsavel);
            elemento.SetAttributeValue("id", this.id);

            return elemento;
        }

        public string generateID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XDocument agenda_XML;
        public MainWindow()
        {
            InitializeComponent();
            this.adicionar_grupo.IsEnabled = false;

            this.agenda_XML = gravar_ou_carregar();
            this.carregar_treeview_do_xml();

        }

        private XDocument gravar_ou_carregar(string nome = "agenda.xml")
        {
            XDocument agenda = null;

            try
            {
                agenda = XDocument.Load("agenda.xml");
            }
            catch (FileNotFoundException)
            {
                agenda = new XDocument(GrupoRaiz.gerar("Lista Telefônica").paraXML());
                agenda.Save("agenda.xml");
            }

            return agenda;
        }


        private void carregar_filho(XElement parente, GrupoRaiz item_atual)
        {
            foreach (XElement elemento in parente.Elements())
            {
                MessageBox.Show(string.Format("Elemento {0} do parente {1}", elemento.Name.ToString(), parente.Name.ToString()));
                int index = -1;
                if (elemento.Name.ToString() == Grupo.tag)
                {
                    item_atual.Items.Add(Grupo.gerar(elemento.Attribute("nome").Value, elemento.Attribute("responsavel").Value, elemento.Attribute("id").Value));
                }

                if (elemento.HasElements && index >= 0)
                {
                    carregar_filho(elemento, (GrupoRaiz)item_atual.Items[index]);
                }
            }
        }

        public void menu_context()
        {
            ContextMenu cm = new ContextMenu();

            cm.Items.Add(new MenuItem { Tag = "Excluir", Header = "Excluir" });
        }

        private static MenuItem AddMenuItem(ContextMenu cm, string text, EventHandler handler)
        {
            MenuItem item = new MenuItem();
            item.Tag
        }
      

        public void carregar_treeview_do_xml()
        {
            treeViewAgenda.Items.Clear();

            List<XElement> lista = this.agenda_XML.Elements().ToList();
            foreach(XElement elemento in lista)
            {
                MessageBox.Show(elemento.Name.ToString() + " Filhos: " + Convert.ToString(elemento.Elements().Count()));
                int index = -1;

                if (elemento.Name.ToString() == Grupo.tag)
                {
                    index = treeViewAgenda.Items.Add(
                        Grupo.gerar(elemento.Attribute("nome").Value, elemento.Attribute("responsavel").Value, elemento.Attribute("id").Value).paraXML()
                        );
                }
                else if (elemento.Name.ToString() == GrupoRaiz.tag)
                {
                    index = treeViewAgenda.Items.Add(GrupoRaiz.gerar("Lista Telefônica"));
                }

                if (elemento.HasElements && index >= 0)
                {
                    this.carregar_filho(elemento, (GrupoRaiz)treeViewAgenda.Items[index]);        
                }
            }
        }
        private void salvar_grupo_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewAgenda.SelectedItem is Grupo)
            {
                Grupo grp = (Grupo)treeViewAgenda.SelectedItem;

                XElement resultado = this.agenda_XML.Descendants(Grupo.tag)
                    .FirstOrDefault(el => el.Attribute("id") != null && el.Attribute("id").Value == grp.id);

                if (resultado != null)
                {
                    Grupo novo_grupo = Grupo.gerar(nome_grupo.Text);
                    resultado.Add(novo_grupo.paraXML());
                    MessageBox.Show("adicionado grupo");
                }

            }
            else if (treeViewAgenda.SelectedItem is GrupoRaiz)
            {
                this.agenda_XML.Element(GrupoRaiz.tag).Add(Grupo.gerar(nome_grupo.Text).paraXML());
            }

            this.agenda_XML.Save("agenda.xml");
            this.carregar_treeview_do_xml();
        }

        private void treeViewAgenda_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            adicionar_grupo.IsEnabled = !(treeViewAgenda.SelectedItem is Pessoa);
        }
    }
}
