using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class swipe : MonoBehaviour
{
    public GameObject scrollbar;    // Reference to the UI Scrollbar object
    private float scroll_pos = 0;   // Current position of the scrollbar
    float[] pos;                    // Array to store positions for each child element
    private bool runIt = false;     // Bool to trigger smooth transition when button is clicked
    private float time;             // Timer to track the transition duration
    private Button takeTheBtn;      // Button that was clicked
    int btnNumber;                  // Index of the clicked button

    // Update is called once per frame
    void Update()
    {
        // Initialize the pos array with the number of child elements
        pos = new float[transform.childCount];

        // Calculate distance between each element in the scrollable list
        float distance = 1f / (pos.Length - 1f);

        // If a button has been clicked and transition should run
        if (runIt)
        {
            // Call the transition handling function
            HandleTransition(distance, pos, takeTheBtn);
            time += Time.deltaTime; // Increment time

            // Stop transition after 1 second
            if (time > 1f)
            {
                time = 0;
                runIt = false;
            }
        }

        // Set the positions of each child object in the scrollable area
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        // Handle scrolling by dragging (mouse down)
        if (Input.GetMouseButton(0))
        {
            // Update scroll_pos with the current scrollbar value while dragging
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            // Snap the scrollbar to the nearest element after releasing the mouse button
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    // Smoothly move the scrollbar to the nearest position
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }

        // Handle scaling of the child objects depending on the scrollbar's position
        for (int i = 0; i < pos.Length; i++)
        {
            Debug.Log("pos.Length" + pos.Length);

            // If the scrollbar is close to the current child, scale it up (focused element)
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                Debug.LogWarning("Current Selected Level" + i);

                // Smoothly scale up the selected child element
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                // Scale down other non-selected elements
                for (int j = 0; j < pos.Length; j++)
                {
                    if (j != i)
                    {
                        transform.GetChild(j).localScale = Vector2.Lerp(transform.GetChild(j).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    // Function to handle transition when a button is clicked
    private void HandleTransition(float distance, float[] pos, Button btn)
    {
        // Loop through all positions
        for (int i = 0; i < pos.Length; i++)
        {
            // If the scrollbar is close to the current position, move it to the button's position
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[btnNumber], 1f * Time.deltaTime);
            }
        }

        // Reset names of all child elements to "." (this seems to be a way to differentiate elements)
        for (int i = 0; i < btn.transform.parent.transform.childCount; i++)
        {
            btn.transform.name = ".";
        }
    }

    // Function to handle button click events
    public void WhichBtnClicked(Button btn)
    {
        btn.transform.name = "clicked"; // Set the clicked button's name to "clicked"

        // Find the index of the clicked button
        for (int i = 0; i < btn.transform.parent.transform.childCount; i++)
        {
            if (btn.transform.parent.transform.GetChild(i).transform.name == "clicked")
            {
                btnNumber = i;   // Save the button's index
                takeTheBtn = btn; // Save the reference to the clicked button
                time = 0;         // Reset the timer
                scroll_pos = pos[btnNumber]; // Update the scroll position to the clicked button's position
                runIt = true;     // Start the transition
            }
        }
    }
}
