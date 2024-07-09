/*
 * 
 * Author:  Roberto Rossi
 * Name:    Red Ods Reader (https://github.com/robertorossi73/RedOdsReader)
 * Web:     http://www.redchar.net
 * Version: 1.1.0
 * 
 * Lightweight library written in C# for reading Open Document Spreadsheet (.ODS) files
 * 
 * MIT License
 * 
 * Copyright (c) 2019 Roberto Rossi
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OPS.Core;


//Gestore file ODS
//TODO : aggiungere metodi/proprietà
//       LastColumn = numero ultima colonna valida nel foglio
//       LastRow   = numero ultima linea valida nel foglio
//       GetCellType() = ritorna il tipo di una cella
//       GetValue() = ritorna il valore di una cella
//
//TODO : ridefinire indici. Invece di partire da 1...N, implementare da 0...N
//
public class RedOdsReader
{
    //definizione di una singola cella
    private class ROdsCell
    {
        public int Row = -1; //numero linea da 1 a N
        public int Column = -1; //colonna da 1 a N
        public string Value = ""; //valore numerico
        public string ValueText = ""; //valore visualizzato
        public string ValueType = ""; //Tipo di dati

        //TODO : manca la gestione delle celle contenenti delle date "date" e "office:date-value"
        //TODO : manca la gestione delle celle contenenti formule "table:formula"
    }

    //definizione di una tabella
    private class ROdsTable
    {
        public List<ROdsCell> Cells = new List<ROdsCell>(); //celle in tabella
        public string Name = ""; //nome tabella 

        //TODO :  manca il supporto per "table:protected"
    }

    string m_xml = "";
    //tabelle presenti nel file ODS
    private List<ROdsTable> m_Tables = null;
    //l'ultima tabelle letta
    private ROdsTable m_CurrentTable = null;

    private string GetNodeAttribute(XmlNode xn, string attribute)
    {
        string result = "";
        XmlNode att;

        att = xn.Attributes.GetNamedItem(attribute);
        if (!(att is null))
        {
            result = att.Value;
        }

        return result;
    }

    private void LoadXml()
    {
        XmlDocument xd = new XmlDocument();
        XmlNodeList tablesList;
        int iRow = 1;
        int iColumn = 1;
        string CellValue = ""; //valore
        string CellValueText = ""; //valore in 
        string CellValueType = ""; //valore visualizzato
        XmlNode att;
        XmlNode textp;
        ROdsTable currentTable;

        xd.LoadXml(m_xml);

        //lista tabelle in file
        tablesList = xd.GetElementsByTagName("table:table");
        foreach (XmlNode tbl in tablesList) //analisi tabelle
        {
            iRow = 1;
            currentTable = new ROdsTable();
            currentTable.Name = GetNodeAttribute(tbl, "table:name");
            foreach (XmlNode item in tbl) //analisi elementi in tabella
            {
                if (item.Name == "table:table-row")
                { //identificata linea
                    iColumn = 0;

                    att = item.Attributes.GetNamedItem("table:number-rows-repeated");
                    if (!(att is null))
                    {
                        iRow = iRow + Convert.ToInt32(att.Value) - 1;
                    }

                    foreach (XmlNode itemRow in item)
                    { //analisi celle in linea
                        if (itemRow.Name == "table:table-cell")
                        {
                            CellValueType = "";
                            CellValue = "";
                            CellValueText = "";

                            att = itemRow.Attributes.GetNamedItem("office:value-type");
                            if (!(att is null))
                            {
                                CellValueType = att.Value;
                            }
                            att = itemRow.Attributes.GetNamedItem("office:value");
                            if (!(att is null))
                            {
                                CellValue = att.Value;
                            }

                            textp = itemRow.FirstChild;
                            if (!(textp is null))
                            {
                                CellValueText = textp.InnerText;
                            }

                            iColumn++;

                            //TODO : manca supporto per tipi diversi
                            if ((CellValue != "") || (CellValueText != ""))
                            {
                                currentTable.Cells.Add(new ROdsCell()
                                {
                                    ValueType = CellValueType,
                                    Value = CellValue,
                                    ValueText = CellValueText,
                                    Row = iRow,
                                    Column = iColumn,
                                });
                            }

                            att = itemRow.Attributes.GetNamedItem("table:number-columns-repeated");
                            if (!(att is null))
                            {
                                iColumn = iColumn + Convert.ToInt32(att.Value) - 1;
                            }

                            att = itemRow.Attributes.GetNamedItem("table:number-columns-spanned");
                            if (!(att is null))
                            {
                                iColumn = iColumn + Convert.ToInt32(att.Value) - 1;
                            }

                        }
                    }
                    iRow++;
                }
                else if (item.Name == "table:table-header-rows")
                {
                    throw new Exception("File type error. Unsupported Rows Header.");
                }
                else if (item.Name == "table:table-row-group")
                {
                    throw new Exception("File type error. Unsupported Row Group.");
                }
            }
            m_Tables.Add(currentTable);
        }
    }

    private ROdsCell GetCellFromCurrentTable(int RowN, int ColN)
    {
        ROdsCell result = null;

        if (m_CurrentTable is null)
        {
            result = null;
        }
        else
        {
            foreach (ROdsCell cell in m_CurrentTable.Cells)
            {
                if ((cell.Row == RowN) && (cell.Column == ColN))
                {
                    result = cell;
                    break;
                }

                //considere l'elenco celle ordinato per riga dalla più bassa alla più alta
                if (cell.Row > RowN)
                {
                    break;
                }

                //TODO: valutare la possibilità di passare alla cella successiva nel caso in cui
                //      la colonna analizzata sia superiore a quella cercata
            }
        }

        return result;
    }

    //ritorna il valore contenuto in una cella. E' possibile richiedere il valore reale 
    // o quello visualizzato
    private string GetCellVal(ROdsCell cell, bool readValueText)
    {
        if (cell is null)
        {
            return "";
        }
        else
        {
            if (readValueText) //valueText
            {
                return cell.ValueText;
            }
            else //value
            {
                return cell.Value;
            }
        }
    }

    //imposta la tabella corrente in base al nome dato
    private void SetCurrentTable(string TableName)
    {
        string name = TableName.ToLower();

        if (!(m_CurrentTable is null))
        {
            if (m_CurrentTable.Name.ToLower() != name)
            {
                m_CurrentTable = null;
            }
        }

        if (m_CurrentTable is null)
        {
            foreach (ROdsTable tbl in m_Tables)
            {
                if (tbl.Name.ToLower() == name)
                {
                    m_CurrentTable = tbl;
                    break;
                }
            }
        }
    }

    //ritorna l'indice dell'ultima linea utilizzata oppure della colonna
    private int GetLastValidColRow(string tblName, bool ReturnLastRow)
    {
        //TODO: l'individiazione dell'ultima linea e colonna valide dovrebbe
        //      essere fatto durante il caricamento del file cosi da rendere
        //      tutto piu rapido.
        int result = 0;

        SetCurrentTable(tblName);
        if (m_CurrentTable is null)
        {
            result = 0;
        }
        else
        {
            foreach (ROdsCell cell in m_CurrentTable.Cells)
            {
                if (ReturnLastRow)
                { //considera le righe
                    if (cell.Row > result)
                    {
                        result = cell.Row;
                    }
                }
                else
                { //considera le colonne
                    if (cell.Column > result)
                    {
                        result = cell.Column;
                    }
                }
            }
        }

        return result;
    }

    //ritorna il numero di tabelle presenti nel file caricato
    public int CountTables
    {
        get
        {
            return m_Tables.Count;
        }
    }

    //ritonra il numero dell'ultima colonna utilizzata
    public int GetLastValidColumn(string TableName)
    {
        return GetLastValidColRow(TableName, false);
    }

    //ritonra il numero dell'ultima riga utilizzata
    public int GetLastValidRow(string TableName)
    {
        return GetLastValidColRow(TableName, true);
    }

    public List<string> GetTablesList()
    {
        List<string> result = new List<string>();

        if (m_Tables != null)
        {
            foreach (ROdsTable tbl in m_Tables)
            {
                result.Add(tbl.Name);
            }
        }

        return result;
    }

    //converte il nome di una colonna nell'indice numerico, A=1
    public int GetColumnIndex(string ColumnName)
    {
        int result = 0;
        ColumnName = ColumnName.ToUpper();

        foreach (char ch in ColumnName)
        {
            result *= 26;
            result += ((int)ch - (int)'A' + 1);
        }
        return result;
    }

    //converte l'indice numerico di una colonna nel suo nome con lettere 1=A
    public string GetColumnName(int ColumnIndex)
    {
        string result = "";
        int ch;
        int ci = ColumnIndex;

        //TODO : introdurre eccezzione al posto della ""?
        if (ci < 1)
        {
            return "";
        }

        while (ci > 0)
        {
            ci -= 1;
            ch = ci % 26;
            result = (char)('A' + ch) + result;
            ci = ci / 26;
        }

        return result;
    }

    //controlla la presenza di una tabella
    public bool ExistTable(string TableName)
    {
        SetCurrentTable(TableName);
        if (m_CurrentTable is null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Ritorna il valore di una cella come string
    /// </summary>
    /// <param name="TableName">nome foglio</param>
    /// <param name="RowN">numero riga 1..N</param>
    /// <param name="ColN">numero colonna 1..N</param>
    /// <returns>Valore stringa contenuto nella cella</returns>
    public string GetCellValueText(string TableName, int RowN, int ColN)
    {
        string result = "";
        string name = TableName.ToLower();
        ROdsCell cell = null;

        SetCurrentTable(TableName);

        cell = GetCellFromCurrentTable(RowN, ColN);
        result = GetCellVal(cell, true);

        return result;
    }

    /// <summary>
    /// Ritorna il valore di una cella convertendo il suo valore stringa in numero decimal
    /// </summary>
    /// <param name="TableName">nome foglio</param>
    /// <param name="RowN">numero riga 1..N</param>
    /// <param name="ColN">numero colonna 1..N</param>
    /// <returns>Valore cella convertito in valore numerico</returns>
    public decimal GetCellValueTextAsDecimal(string TableName, int RowN, int ColN)
    {
        string val;
        decimal res = 0;
        var nfi = new NumberFormatInfo { CurrencyDecimalSeparator = "." };

        val = this.GetCellValueText(TableName, RowN, ColN);
        val = val.Replace(',', '.');

        decimal.TryParse(val, NumberStyles.Currency, nfi, out res);

        return res;
    }

    //carica i dati presenti nel file specificato
    public bool LoadFile(string filePath)
    {
        bool result = false;
        string targetFileName = "content.xml";

        m_Tables = new List<ROdsTable>();
        m_CurrentTable = null;

        //TODO : se il file è aperto genera un errore. Si potrebbe evitare la cosa usando una copia temporanea
        //TODO : evitare eccezzione se il file ods non esiste?
        using (System.IO.Compression.ZipArchive za = System.IO.Compression.ZipFile.OpenRead(filePath))
        {
            foreach (System.IO.Compression.ZipArchiveEntry entry in za.Entries)
            {
                if (entry.FullName.ToUpper() == targetFileName.ToUpper())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(entry.Open(), Encoding.UTF8))
                    {
                        this.m_xml = sr.ReadToEnd();
                        result = true;
                    }
                    break;
                }
            }
        }

        this.LoadXml();
        return result;
    }

}
