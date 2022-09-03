[Release](https://github.com/randomuserhi/AnimeDownloader/releases/tag/1.1.1)

# AnimeDownloader
A small browser to help download anime series from 9anime.id .

# How to use
Select the anime of your choice in the browser on the left and click `Get Episodes` to get the list of episodes
![image](https://user-images.githubusercontent.com/40913834/188254289-87a78650-319d-4fcf-b01a-82b772bdc3ea.png)

Select the episodes to add to the download queue and click `Add Selected`.
![image](https://user-images.githubusercontent.com/40913834/188254304-68acc797-81b5-49d7-85a6-5e648eb8f4b5.png)

Hit `Start Queue` and your selected anime's will download into your anime folder!

# File structure
```
Anime
- Sub
    - episode1.mp4
    - episode2.mp4
    ...
    - autodownloader.ini
- Dub
    - episode1.mp4
    - episode2.mp4
    ...
    - autodownloader.ini
...
```

Note that `autodownloader.ini` is just some metadata for the program to use to keep track of what episodes you have downloaded already such that it does not fetch duplicate ones between runs.

# Known Drawbacks
- Only supports `9anime`.
- Can't download other media (mangas).
- Currently only works with `mp4Upload`, support for other mirrors may be added in the future.
- Only works on windows due to reliance on `cefsharp`.
    - I am looking into making an android, ios and UWP version in xamarin
