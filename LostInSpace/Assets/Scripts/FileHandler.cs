using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FileHandler:MonoBehaviour {

    public static FileHandler self;
    public string path;

    private StreamWriter writer;

    private void Awake()
    {
        if(self==null)
        {
            self = this;
        }
        if(self!=this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        writer = new StreamWriter(path, true);
    }
    private void OnDisable()
    {
        writer.Close();
    }

    public void WriteString(string message)
    {

        //Write some text to the test.txt file
        //StreamWriter writer = new StreamWriter(path, true);
        //writer.WriteLine(message);
        writer.Write(message);
        //writer.Close();
    }
}
