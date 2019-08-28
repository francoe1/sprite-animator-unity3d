using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteAnimatorEditor
{
    [Serializable]
    internal class GUIResources : ScriptableObject
    {
        [HideInInspector]
        public Color[] Colors;
        [HideInInspector]
        public Texture2D[] Textures;
        [HideInInspector]
        public Texture2D[] TexturesDarken;
        [HideInInspector]
        public Texture2D[] TexturesLigthen;
        [HideInInspector]
        public Texture2D[] Icon;

        //Styles
        [HideInInspector]
        public GUIStyle Background0;
        [HideInInspector]
        public GUIStyle Background1;
        [HideInInspector]
        public GUIStyle Background2;
        [HideInInspector]
        public GUIStyle Background3;
        [HideInInspector]
        public GUIStyle Background4;
        [HideInInspector]
        public GUIStyle Background5;
        [HideInInspector]
        public GUIStyle Background8;
        [HideInInspector]
        public GUIStyle TimeLine;

        [HideInInspector]
        public GUIStyle Field;
        [HideInInspector]
        public GUIStyle SeparatorField;
        [HideInInspector]
        public GUIStyle LibraryInfo;
        [HideInInspector]
        public GUIStyle FieldValue;
        [HideInInspector]
        public GUIStyle FieldEnum;

        [HideInInspector]
        public GUIStyle LabelDate;
        [HideInInspector]
        public GUIStyle LabelSearch;
        [HideInInspector]
        public GUIStyle LabelPivot;
        [HideInInspector]
        public GUIStyle LabelTimeLine;

        [HideInInspector]
        public GUIStyle ToggleBackgroundFalse;
        [HideInInspector]
        public GUIStyle ToggleBackgroundTrue;
        [HideInInspector]
        public GUIStyle ToggleBackgroundControl;

        [HideInInspector]
        public GUIStyle ButtonDefault;
        [HideInInspector]
        public GUIStyle ButtonNonMargin;
        [HideInInspector]
        public GUIStyle ButtonAlert;
        [HideInInspector]
        public GUIStyle ButtonAlternative;
        [HideInInspector]
        public GUIStyle ButtonGreen;
        [HideInInspector]
        public GUIStyle ButtonMenuContext;

        [HideInInspector]
        public GUIStyle Toolbar;
        [HideInInspector]
        public GUIStyle ToolbarButton;

        [HideInInspector]
        public GUIStyle TreeViewFolder;
        [HideInInspector]
        public GUIStyle TreeViewFolderSelected;
        [HideInInspector]
        public GUIStyle TreeViewAnimation;
        [HideInInspector]
        public GUIStyle TreeViewAnimationSelected;
        [HideInInspector]
        public GUIStyle Header0;
        [HideInInspector]
        public GUIStyle Tooltip;
        
        private string[] m_images = new string[]
        {
            //Play 0
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxMWCiEMCf5fAAAArElEQVRIx63VIQ+BURQG4I8pmqAImuIP+AeSrumaLPoNkiRpukSQBdEPsEk2wSbYPArdvtfp99nuveectyg+ZWGnU5QvR9yNMwA2WhnA1TADYKWRAZz1M4CXuXoCwEkvA3iaqSUAHHQzgIeJSgLAVjsDuBllAKw1M4CLQQbA9Huu+mujpd/4xytEjxh+Y9RIYStHwxSOc7RQwpUWLtVorYfBEkVbFK5L+zLx/gb4ORor9ZM9TAAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxOC0xMS0xOVQyMjoxMDozMyswMTowMGwF03EAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTgtMTEtMTlUMjI6MTA6MzMrMDE6MDAdWGvNAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAABJRU5ErkJggg==",
            //Rewin Left 1
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACXBIWXMAAA3XAAAN1wFCKJt4AAAAAmJLR0QAAKqNIzIAAAAHdElNRQfiCxMWEx9WaErsAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDE4LTExLTE5VDIyOjE5OjMxKzAxOjAwB4/pkgAAACV0RVh0ZGF0ZTptb2RpZnkAMjAxOC0xMS0xOVQyMjoxOTozMSswMTowMHbSUS4AAAAZdEVYdFNvZnR3YXJlAHd3dy5pbmtzY2FwZS5vcmeb7jwaAAABQElEQVRYR+2WMUoEMRSGR220U7Sw0cILiBcQbMTKWrDxEIKNN7ARbO23EbyBYOENxAMIYrliIYjg+L3kH3WcndkkikGcDz7IvLy3+9hsMil6esqyXNYwCeqXNEyDD3jCc1xQKBhq1vEFD3FK4TgorLjHLYWDsHxX6bnCFU2F42trnOCMpjsh73MDxiPuaToMX9fgBteU0go5XxuoOMN5pXXj80fyjAc4qdQGzLU1YNzhplLb8bmdXOLInUK8qwHjFY9xWiVNXNp4HnBXJe8QG9dAxTWuqqyOnw9mgLMqjWnAsCXdx/qSuqk4bnFDtTENVFzgx+HlY9HY2h7htnuKZ4g79v0T9uQ6SWOIc36YxGnrFvs17BdI4MeW4M/9CbNtw6wHUbajOOvLKOvrONuFJNuVLPuldFHDJL5b3/PfKYo3WdfS7hhFXdAAAAAASUVORK5CYII=",
            //Rewin Rigth 2
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxMWEx9WaErsAAAA5UlEQVRIx+2UPQ4BURRGb0ZDJ6HQjMIGxAYkGplKLdFYhERjBxqJ0gI0EjuQKOxALEAiOkQhEYmj8GJmPJfivdLXnntP3u8V+edHKDlwMgy4UVd5kTkXvb3CCoBI4RF7AK29yxk0ATnGhn4SUGBGHEtAjU2M7fYmO9AEBPS5JnG6OcuIO2gCyizfaFJAlTV2XgI6nGwcL62XXlpaQJ7pR/oUELJASyRCg62GRYQ2R/S0GFrnkhQw4XsO33Eg7nHcgodD9HCNHh6Sl6fs4TN5+c6myGWgmCK3kSbiPFRNUfiDl+UfeQAWmweKNvzTHgAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxOC0xMS0xOVQyMjoxOTozMSswMTowMAeP6ZIAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTgtMTEtMTlUMjI6MTk6MzErMDE6MDB20lEuAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAABJRU5ErkJggg==",
            //Folder Close 3
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxQFIQnMe45cAAAAfUlEQVRIx+2VMQqAMAxFf8VNh17VC7i49Zjq5mAFO2iHeIGaBoPo0LeG/yCfQAAlBiALm5ytZhMYqKeT0hzUSQQ73TPn8xUaZtpKBNoSibj9MLLpBQMvyDNpBfoOakiO5U2+76AIiqAI/iMIqnyo4BAfxyMc91zzeOO1FeACJzFS3aaQ6isAAAAldEVYdGRhdGU6Y3JlYXRlADIwMTgtMTEtMjBUMDU6MzM6MDkrMDE6MDDnXR6xAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDE4LTExLTIwVDA1OjMzOjA5KzAxOjAwlgCmDQAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAAASUVORK5CYII=",
            //Folder Open 4
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAAAejAAAHowEwL7LFAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAPxQTFRF//////8A27aS379A48ZV885Jz7+H789I6bwX7MxN8c9I0b+E785J8+/b8uat8M1J7LoW785L785K8M1L785K5Lw767oV789K781K8M5L7MAm7L8k8+ay8uGg7MEx7MI08dyM785K9OzO9OzO6sE17sg68dp547w447w08M5K9O7W78pC8dVo1c2q785K1cyq2suQ67oW785K9O7Z9O7a8NFZ5eDL9O/b39rF2dS+785K67sW6bsc08648NBQzciz8NBP67oW785K785K9O/c785K785Lzsmuz8qv0b+G6OPO6eTO6eTR67oW6+bT7ejU785K7+rW8OvY9O/cks+g7QAAAEd0Uk5TAAEHCAkVICAiKDU4P0BRV11tboWcqLKztLW9v8DBxcbGy8vMzc3O0NTX2Nnb3+Dh4ePk5Obq6+3u8fHy8/T19/f4+Pv8/f3UX09jAAAA4klEQVQ4T73SWVeBURTG8aNIpDJFmmTKVKEtQ0goZT7s7/9des9+F63W8rS6qf/t87va5yj190V6m/xbweyr1v520M6tOzn43o4NsiPU3q/BHYMObVD6+BmU3y2wmjIv58yLBfN8yTxdbUB6PJ5wvdYlanSImk2iToOoWyOrI7dTzeKWTL5oUN4GN2jXCQHVIQRBARdwH7gE3EJwqgwowF0fC7iE+9OuAa+PEITNoeIpuGufgGu43zsMuHqGICTPfQ537RGQgXtC/mTlDYKAgDO4W2c2FSGI2t8+1gc9eNU/9QlG785Blnh1MwAAAABJRU5ErkJggg==",
            //Delete 5
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgBAMAAACBVGfHAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAAADdAAAA3QFwU6IHAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAABVQTFRF////3yAg4xwc4hwc4hoa4hsb4hsb8kqBCQAAAAZ0Uk5TAAhIsLj4WlKcDAAAAJJJREFUKFNl0rERgCAMBVBwAgsH4M5jAQt7GwewcAEl+48gIfycx7cJwgNJYgghbqE/69zCUpK9T/ehIZ5y2USWV8kiYmS6RY4GxEiug0oqMKJAya5BSW6Dp8eSDNQVDLAAWgBwmDgAcfAnyMFJT8EJgBMAnqAt46H02fFidPUxOUqfCkQlpCJTG6hR3Epq9vA7fNWJdJ05tsjeAAAAAElFTkSuQmCC",
            //Back 6
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxMWKyTw/B8zAAAAZUlEQVRIx+2VOwqAUAwEFz2xH/QYXsJ7jpUg8oqXrI2Q6TOQZkdqwMShPMwAaQULNxkFK0+iCjbeRBTstOhXcIKlYCxFKbKKodMWx3yhzn96/sWkuaNqz7pkh0Wy0ybZcZVieb8AaTSJ1iNqyUoAAAAldEVYdGRhdGU6Y3JlYXRlADIwMTgtMTEtMTlUMjI6NDM6MzYrMDE6MDCkXeuXAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDE4LTExLTE5VDIyOjQzOjM2KzAxOjAw1QBTKwAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAAASUVORK5CYII=",
            //Forward 7 
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACXBIWXMAAA3XAAAN1wFCKJt4AAAAAmJLR0QAAKqNIzIAAAAHdElNRQfiCxMWKyTw/B8zAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDE4LTExLTE5VDIyOjQzOjM2KzAxOjAwpF3rlwAAACV0RVh0ZGF0ZTptb2RpZnkAMjAxOC0xMS0xOVQyMjo0MzozNiswMTowMNUAUysAAAAZdEVYdFNvZnR3YXJlAHd3dy5pbmtzY2FwZS5vcmeb7jwaAAAAxUlEQVRYR+2XCwrCMBBEEz2xH/QYeghv5IHiJM5AhVK2zUeFfRBK2mbepoXSDa1JKd0wDpyOhXJx5OkxQDiVixMv9wWiObk487Y+QLAkFxfe3hYEW+TiymVtQOAaeebBpfUgbLUcY8/ldSDI5VZcXg+CXG5ls3zH4++BHX3vFQgEehEFBHoRBQR6EQUE/mcRXPrB5i9hjDF3P/f3zMSTx7ZgZ5Yn0ee3XECwVETfxkRANFfEmNZMQDgtYmxzKiDORRjb8xBe0WcSTBQRQgsAAAAASUVORK5CYII=",
            //Add 8 
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAAAEKAAABCgEWpLzLAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAEVQTFRF////QL//Sbb/YL/fVcrqUszrUsXvUMfvT8buUMfuUcnuUcfuT8nwT8fwUcjuUcjvUMjuUMjvUMjvUMjvUMjvUMjvUMjvn2pnzQAAABZ0Uk5TAAQHCBgZHyAtSWh7hJeYwcja4/Lz9a413CcAAAC0SURBVDjLhVNZFoMgEBtFqyiLCMz9j1qFPtGiJj8wL2HJLEQHWmmsC8FZI1uq0WvPB7zu/2ihIl8QlTjz3cIVlq7w48o3WMfj/C2/KX53iHL/PG2Yyyv5H6qcGfZ4KLFK/uKzIO5uNT8LWG/5828C35LkNwFLMufwUwkM2exvSGh2QZP32a0ll9aprt6UCEfhXRCwAD4BPwltwkTBVMNi4XLDhsEtB5sWtz0eHDx6eHifx/8LuS01q9/Sc8MAAAAASUVORK5CYII=",
            //ListEnum 9
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxYFIQgRdXZBAAAAdklEQVRIx+3Puw2AMAyE4YOhqDKAB8jSZqUUR4UQystxKlCu/7/EwNofxkCld8qwUXFMvH/u8ycIk/uERAHA6CQS4/0LD/HkLuKdDxN5PkSUczNRz01EO+8S/bxJ2PIqYc+LxFieEeP5i/DlAEChUinOfO07uwCXo0WGz5GOxgAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxOC0xMS0yMlQwNTozMzowOCswMTowMMaKMGYAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTgtMTEtMjJUMDU6MzM6MDgrMDE6MDC314jaAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAABJRU5ErkJggg==",
            //Area Sprite 10
            "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAQAAAAAYLlVAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxYBAwsox8kHAAADvElEQVRo3tWZQUhcVxiFvztmwgwNaWQEMTUhxK6mtBQXQTRDF60lMSBNcZGKq4YudJWNIQ1tcdFutIRugiC0IatYIiRMarHUbrQFV6FM0FVbiph0o1VLw2iaeLrwzrw3ec+SQe+d9MAP979c3jn33H/mPe6PbuppFNWHA6hPxQjXTRSHG04E3Iij2gdAjqXyun4uYlwIwADDjJbzZmZhW8CS+b2sc9UJeQmrISagJCCMg8BJdZifQEO0AXDPDIJa+Zg0f/OJWVCSL2hB5M0oqIf3SQArDJoHynCFRrb4ykyA+unG8CsXdjoZSToWyr+RJI2B0tq0B/VEDaBhm10GZe14EUAz5SPtBXXZ8QyAFm2W1bikSyGmY0ENhLEJPOBDMEXlaAVg3iwDnzFPiofcArOgt2lBzALQxykM8Ce3ge94l0a2mALgFDngN7OgZ3TgI0m3XBz/fznQHyq9DuuCG7wR8qF+W0f0z0G65sSBazFMxX18wJmnfvdFRpzsfgRIV2piMhgnlXVCG+dFVsno5FVJnV7oOyVdLWWJ8nyLDfeoYErs4kF7gudIwDTrzHnhnGOd6Zh5eXPDH1NVqk4rryYvTE3K63R0+o6k814EnJd0p5QFp5G04R4VTIGAFRvuEc+kw+qVFweUVK8Oe9lqdboy6lKdF6Y6dSkTnb4u6awXAWclXS9lQRE22nCPCqZAwJYN94hnUo9mdNQHv45qRj1etvr/gvq16OezVFktqr+UBUXYzRFyXvaa4wjdUQHGhntUMNX82yQQsGTDPeKZdEDt8nIEMmrXAS9brU7XcQ0o5YUppQEdj06PSzrnRcA5SeOlLCjCQzbco4IpELCJy5uRMOKZ1KGxmO8UB1BGY+rwstVqlaV3/4xdMGlImzrhhf6ENjVUyoIibGO/vZZ0jVb22ytgnquX0RyPuOuF8y6PYq9CalyENUet/oiM+uxV7UleYpl1FlhhxCw4IM4ySIYsL9LAfX5k+6q29pfVKWCYVQ7yGnVs8AKduCmSNPA9D0nzmAJ/Uc9FUtGGxaXw23pPHXjGlk2wIMcrwJz5GdREF0lgjdtmQ4YzNCN+ML+AXuZN4D6TRkrxDoeAf/jW/AF6nTZg3szuTLODA2rQE0nSmhKgfPnUBkDtdlwAUMFm7aCB8ro8KKG1UNOrOgfMsq7wKjBttoBRjHVgCijwJc2I7Q7r57yHYYkCMMXX1oFRMFv6lLeAe2a5agf81kBzpJXkCvWhzTYHDtSweZ1gIrJyI9RK2ktMshGZm/gX4uraOLssgLIAAAAldEVYdGRhdGU6Y3JlYXRlADIwMTgtMTEtMjJUMDE6MDM6MTErMDE6MDDG43m8AAAAJXRFWHRkYXRlOm1vZGlmeQAyMDE4LTExLTIyVDAxOjAzOjExKzAxOjAwt77BAAAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAAASUVORK5CYII=",
            //SearchIcon 11
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAQAAADZc7J/AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiCxYBFxX8ZiMxAAACJ0lEQVRIx6WTPUhVcRiHn+PH9Zqhix8QlFIEZktXhyKClnCqhjAyQhoaAhva+qBJaLFySjJqCRorBycpiIYQLfTiUGYOJdWQRUJp+NH1abgnuXTP0Qv3N52P//Oc9/++5x+wYSzlEClStNLEFBOkeR1MUGhsccz8PLauELjUKy7lYAs513N2bIYnHQkXf/KSR6wFt3vc6/4Mn9/ZWHAjXHbfagC3utcEgI0+Dd/FV+EBM2rGY2CZ13xnRl0xbReAV8ONRPfCpFOq3gKbHf+vhUPWgs+y7YwWnFX1vZVWOh0xhSGwyV+qtkYJbqt6GuwzOl1gr6rnowTZ/u+03OUYQRo8oerdqPkvqj/AlHFZMWGjqq9K8gyNbAGmgJbYMZWzO5hlEdiTL/jCKtAEfIgVrDFrHVXAxzxBsMwbYJsNTLIWI5gJFsj2f6Ik4nX2tLUFiwzGCB4CbQCko6bQrepzA+v9FtHCccusdlbVw1GCuhDrBtvzFNM2g/dUHbM0skJPhQe4Gaz3kZkQXrbPSvCoqkvmz8kaB0yAg6HiggFY5UHPmLIcTHozFF6OwkfVfrDBr+FXX9j+79S5w4710zGSV36Iq3aCDT7J2flnX/p9/e6PvSbjcV2wE8CTzkVM4a3744rPTb8JsNaLPnDSVfW3ow54zopCcNWBnBUV7ooZWSw+ag2FxIqicLCnKBycKQoH54vCweGicHCfK0XgoWLYeWfsifhFNs1f0ZQzmg/0PpgAAAAldEVYdGRhdGU6Y3JlYXRlADIwMTgtMTEtMjJUMDE6MjM6MjErMDE6MDAQAMeeAAAAJXRFWHRkYXRlOm1vZGlmeQAyMDE4LTExLTIyVDAxOjIzOjIxKzAxOjAwYV1/IgAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAAASUVORK5CYII=",
            //Star 12
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAAAg5AAAIOQH6aNKkAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAALdQTFRF//////8A//+A/6pV/79A/9tJ379A48ZV5sxN6tVA69hO7chJ789Q8dVH8slR68xH7NBM8cxK7s1L7s9I7s9L785L781L8M1K7s5L7s9K785J8M5L8M9J7s5J785K785K789K8M5K789J789K8M1K785K785K781K7s1K785K785K785K8M5J785K785K785K781K785K785K785K781K8M5K785K785K785K785K785L785K785KmYbjzAAAADx0Uk5TAAECAwQHCAkKDA0OEBITGRs3PUpLTlJWWFpiY2Vpcnx/h4+UlayytLm6vcLGycrMzs/Q0dPW2uPm6/P1hCkhrQAAAOdJREFUGBl9wQWWwjAABcAftFiB4m6La3H59z/XltLHS0LTGWjicURrtRApdrnEEKVBNhBBnMmzgFmdnjrMXHpcGNXoq8FkT98eBhUGKggjSlsGtiUBWdFpj6anFyWv03TUdorwDNwnjZ7uAPaBEQ42kN/RaJeHJ7ehwSYHX2bNUOsMAtaKIVYWvtIL/likIUnOqZknobCpsaEqU1OGqkNNB6oxNWOoltQsobpTc4eiwI9jtXrkRwEyh2/XpgBE88o3B7IeyVs3AV+ieyPZg+yPj34KX6n+gxPIZsMsFNnhDDILPyz4/gHdJWKkTYQ/MgAAAABJRU5ErkJggg==",
            //App Icon 13
            "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAAABvAAAAbwHxotxDAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAASZQTFRF////AAAAAAAAAABAKDx5HSRfJzt7Jjp6KDx6GCBaGB9aJjp7Jzt7GiBcGR5YJzt6Jjh3FxpUFhlSHilmJzt6FxtUExRNEhFJEhJKExJKFBFJFxpUFxtUHytnIBVGIzNwIzNxJR9GJjl4Jzt6KDt6QDZKR0hcTE9sU0M8WCA0WVVnWyEzbyUtey4yh25VlEcxlnA0pXgzsoE/uUAayS8AyjQHyzMFyzQHzDIDzIsnzJAyzaUvzzUFz6g/0JQ50Kgv0a1G1JY41KUt2akj2rVk3sNv4JES4ZUb45Ya45cc5p8w6qIv66Mm7J0w7Wco77VI77ZJ8EwV8Ioo8Kwp8Yoo8owU80wU88FZ9HAq9sMu98Uv+Loi+MQs/Np9/tps/1QZ/8Yb/8cexnZ9cwAAABd0Uk5TAAECBCZGcH+AsrOztbrC5ufq6/D2+f3f4ZCuAAAAvUlEQVQYGQXB0U6DMBSA4f+0p4BjAdQsXPr+T+SVmszEaHZj0TmgPa3fJwCqXsmWMyAgYRgBiEuqCNLPIBWAr2t1hBn0ZADMAdUB+sd2MDl9nGX4Vh1pp4O5qfW2N+OP80h3hKZh/UxbVX84cG3uaqpiT6+lmgLaTVHKdrlBDi4DFtr2PqzhAcwZAADkoye5HCG/vxSevTubrJqXkYvb3kqsFWISpJ/ZdwDYY/GQfukxwC1/BQFQ9Z1PdkvAPyBSU85S36NhAAAAAElFTkSuQmCC",
            //Arrow LEFT 14
            "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACXBIWXMAAA3XAAAN1wFCKJt4AAAAAmJLR0QAAKqNIzIAAAAHdElNRQfiCxwMOSGxsK2fAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDE4LTExLTI4VDEyOjU3OjMzKzAxOjAwv5NURAAAACV0RVh0ZGF0ZTptb2RpZnkAMjAxOC0xMS0yOFQxMjo1NzozMyswMTowMM7O7PgAAAAZdEVYdFNvZnR3YXJlAHd3dy5pbmtzY2FwZS5vcmeb7jwaAAABVUlEQVRIS62WP1ICMRjFNxaUaM0JwMaxBr0DnMBOsPIG6iWUU+AdRGuGAvQE9FDSxN/L7G7Bn+xmkzfzm3zzhvneJvkWyFLKWmvgER5yK7vI12jRtMvyBVO4kidFB9C4BS+USxg4M5Vo3IcVHOo5/0izHdCgDe+Uc7h25hkFB9B4yPILEzDyfKodQOMOzChFx5k1VBlAU42enlZPracPkjeAxjpfnbPOuy0vVCcDaKzRe6VcQN+ZDXUUQGPNsmZas92SF6MygMaXoLdQb6PeyiSqvORYlQHGmC2MKe/hz5kJdLQDQr5ZbuAN9vJidPKICNmDpugWfpzZUN47IGTNcgdPsJMXqspLJsTCB2UPPp0ZoMqAQoRsYEQpNs6sodoBhQjRLrQb7crK8yk4QCJkB7oX3Y/u6awaBRQiRBOmSdPERY+0V3zNdGEOUvmTmVQ0PvjbkmX/ZT6R6hNO8sUAAAAASUVORK5CYII=",
        };
        
        public void Load()
        {
            if (Application.isPlaying) return;

            try
            {


                Colors = new Color[]
                {
                CreateColor(68, 68, 68), //0
                CreateColor(51, 51, 51), //1
                CreateColor(77, 124, 255), //2
                CreateColor(20, 20, 20), //3
                CreateColor(42, 219, 138), //4
                CreateColor(41, 41, 41),//5
                CreateColor(120, 120, 120), //6
                CreateColor(178, 151, 9), //7
                CreateColor(178, 157, 9), //8
                };

                Icon = new Texture2D[]
                {
                (LoadTexture(m_images[0], "PlayOff")),
                (LoadTexture(m_images[0], "PlayOn", Colors[4])),
                (LoadTexture(m_images[1], "PlayRigthOff")),
                (LoadTexture(m_images[2], "PlayLeftOff")),
                (LoadTexture(m_images[1], "PlayRigthOn", Colors[4])),
                (LoadTexture(m_images[2], "PlayLeftOn", Colors[4])),
                (LoadTexture(m_images[3], "FolderClose", Colors[4].Darken(45))),
                (LoadTexture(m_images[3], "FolderOpen", Colors[4])),
                (LoadTexture(m_images[5], "Delete")),
                (LoadTexture(m_images[6], "Backward")),
                (LoadTexture(m_images[7], "Forward")),
                (LoadTexture(m_images[8], "Add")),
                (LoadTexture(m_images[9], "ListEnum")),
                (LoadTexture(m_images[10], "SpriteArea")),
                (LoadTexture(m_images[11], "Search")),
                (LoadTexture(m_images[12], "Star")),
                (LoadTexture(m_images[13], "AppIcon")),
                (LoadTexture(m_images[14], "ArrowLeft", Colors[4])),
                };

                TexturesDarken = new Texture2D[]
                {
                CreateTexture(Colors[0].Darken(25), "Text1-Dark"),
                CreateTexture(Colors[1].Darken(25), "Text2-Dark"),
                CreateTexture(Colors[2].Darken(25), "Text3-Dark"),
                CreateTexture(Colors[3].Darken(25), "Text4-Dark"),
                CreateTexture(Colors[4].Darken(25), "Text5-Dark"),
                CreateTexture(Colors[5].Darken(25), "Text6-Dark"),
                CreateTexture(Colors[6].Darken(25), "Text7-Dark"),
                CreateTexture(Colors[7].Darken(25), "Text8-Dark"),
                CreateTexture(Colors[8].Darken(25), "Text9-Dark"),
                };

                TexturesLigthen = new Texture2D[]
                {
                CreateTexture(Colors[0].Ligthen(35), "Text1-Ligthen"),
                CreateTexture(Colors[1].Ligthen(35), "Text2-Ligthen"),
                CreateTexture(Colors[2].Ligthen(35), "Text3-Ligthen"),
                CreateTexture(Colors[3].Ligthen(35), "Text4-Ligthen"),
                CreateTexture(Colors[4].Ligthen(35), "Text5-Ligthen"),
                CreateTexture(Colors[5].Ligthen(35), "Text6-Ligthen"),
                CreateTexture(Colors[6].Ligthen(35), "Text7-Ligthen"),
                CreateTexture(Colors[7].Ligthen(35), "Text8-Ligthen"),
                CreateTexture(Colors[8].Ligthen(35), "Text9-Ligthen"),
                };

                Textures = new Texture2D[]
                {
                CreateTexture(Colors[0], "Text1"),
                CreateTexture(Colors[1], "Text2"),
                CreateTexture(Colors[2], "Text3"),
                CreateTexture(Colors[3], "Text4"),
                CreateTexture(Colors[4], "Text5"),
                CreateTexture(Colors[5], "Text6"),
                CreateTexture(Colors[6], "Text7"),
                CreateTexture(Colors[7], "Text8"),
                CreateTexture(Colors[8], "Text9"),
                };

                LoadStyles();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

            }
            catch
            {
                Debug.LogWarning("Error load Resources");
            }

        }

        private void LoadStyles()
        {
            {
                LabelSearch = new GUIStyle();
                LabelSearch.normal.background = Textures[0];
                LabelSearch.normal.textColor = Color.white;
                LabelSearch.alignment = TextAnchor.MiddleLeft;
                LabelSearch.fontSize = 10;
                LabelSearch.fontStyle = FontStyle.Bold;
                LabelSearch.padding = new RectOffset(25, 10, 10, 10);
                LabelSearch.margin = new RectOffset(0, 0, 1, 1);
            }

            {
                LabelTimeLine = new GUIStyle();
                LabelTimeLine.normal.background = Textures[3];
                LabelTimeLine.normal.textColor = Color.white;
                LabelTimeLine.alignment = TextAnchor.MiddleCenter;
                LabelTimeLine.fontSize = 10;
                LabelTimeLine.fontStyle = FontStyle.Bold;
                LabelTimeLine.padding = new RectOffset();
                LabelTimeLine.margin = new RectOffset();
            }

            RectOffset PaddingElement = new RectOffset(0, 0, 2, 0);

            {
                TreeViewAnimationSelected = new GUIStyle();
                TreeViewAnimationSelected.normal.background = Textures[2];
                TreeViewAnimationSelected.normal.textColor = Color.white;
                TreeViewAnimationSelected.padding = PaddingElement;
                TreeViewAnimationSelected.alignment = TextAnchor.MiddleLeft;
                TreeViewAnimationSelected.padding = new RectOffset(4, 0, 0, 2);
            }

            {
                TreeViewAnimation = new GUIStyle();
                TreeViewAnimation.normal.background = TexturesLigthen[5];
                TreeViewAnimation.normal.textColor = Color.white;
                TreeViewAnimation.padding = PaddingElement;
                TreeViewAnimation.alignment = TextAnchor.MiddleLeft;
                TreeViewAnimation.padding = new RectOffset(4, 0, 0, 2);
            }

            {
                TreeViewFolderSelected = new GUIStyle();
                TreeViewFolderSelected.normal.background = Textures[2];
                TreeViewFolderSelected.normal.textColor = Color.white;
                TreeViewFolderSelected.padding = PaddingElement;
                TreeViewFolderSelected.alignment = TextAnchor.MiddleLeft;
                TreeViewFolderSelected.padding = new RectOffset(21, 0, 0, 0);
            }

            {
                TreeViewFolder = new GUIStyle();
                TreeViewFolder.normal.background = TexturesDarken[0];
                TreeViewFolder.normal.textColor = Color.white;
                TreeViewFolder.padding = PaddingElement;
                TreeViewFolder.alignment = TextAnchor.MiddleLeft;
                TreeViewFolder.padding = new RectOffset(21, 0, 0, 0);
            }

            {
                Toolbar = new GUIStyle();
                Toolbar.normal.background = Textures[1];
                Toolbar.normal.textColor = Color.white;
                Toolbar.alignment = TextAnchor.MiddleLeft;
                Toolbar.fontSize = 10;
                Toolbar.fontStyle = FontStyle.Bold;
                Toolbar.padding = new RectOffset(0, 0, 0, 0);
                Toolbar.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                Header0 = new GUIStyle();
                Header0.normal.background = Textures[1];
                Header0.normal.textColor = Color.white;
                Header0.alignment = TextAnchor.MiddleCenter;
                Header0.fontSize = 10;
                Header0.fontStyle = FontStyle.Bold;
                Header0.padding = new RectOffset(0, 0, 0, 0);
                Header0.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                Tooltip = new GUIStyle();
                Tooltip.normal.background = Textures[3];
                Tooltip.normal.textColor = Color.white;
                Tooltip.alignment = TextAnchor.MiddleLeft;
                Tooltip.fontSize = 10;
                Tooltip.fontStyle = FontStyle.Normal;
                Tooltip.padding = new RectOffset(3, 3, 3, 3);
            }

            {
                ToolbarButton = new GUIStyle();
                ToolbarButton.normal.background = Textures[0];
                ToolbarButton.normal.textColor = Color.white;
                ToolbarButton.hover.background = Textures[2];
                ToolbarButton.hover.textColor = Color.white;
                ToolbarButton.alignment = TextAnchor.MiddleCenter;
                ToolbarButton.fontSize = 10;
                ToolbarButton.fontStyle = FontStyle.Bold;
                ToolbarButton.padding = new RectOffset(10, 10, 0, 0);
                ToolbarButton.margin = new RectOffset(0, 0, 0, 0);
                ToolbarButton.stretchHeight = true;
                ToolbarButton.stretchWidth = false;
            }

            {
                ButtonDefault = new GUIStyle();
                ButtonDefault.normal.background = Textures[4];
                ButtonDefault.normal.textColor = Color.white;
                ButtonDefault.hover.background = TexturesDarken[4];
                ButtonDefault.hover.textColor = Color.white;
                ButtonDefault.active.background = TexturesLigthen[4];
                ButtonDefault.active.textColor = Color.white;
                ButtonDefault.alignment = TextAnchor.MiddleCenter;
                ButtonDefault.fontSize = 12;
                ButtonDefault.fontStyle = FontStyle.Bold;
                ButtonDefault.padding = new RectOffset(5, 5, 10, 10);
                ButtonDefault.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                ButtonNonMargin = new GUIStyle();
                ButtonNonMargin.normal.background = TexturesDarken[4];
                ButtonNonMargin.normal.textColor = Color.white;
                ButtonNonMargin.hover.background = Textures[4];
                ButtonNonMargin.hover.textColor = Color.white;
                ButtonNonMargin.active.background = TexturesLigthen[4];
                ButtonNonMargin.active.textColor = Color.white;
                ButtonNonMargin.alignment = TextAnchor.MiddleCenter;
                ButtonNonMargin.fontSize = 12;
                ButtonNonMargin.fontStyle = FontStyle.Bold;
                ButtonNonMargin.padding = new RectOffset(2, 2, 2, 2);
                ButtonNonMargin.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                ButtonMenuContext = new GUIStyle();
                ButtonMenuContext.normal.background = TexturesDarken[4];
                ButtonMenuContext.normal.textColor = Color.white;
                ButtonMenuContext.hover.background = Textures[4];
                ButtonMenuContext.hover.textColor = Color.white;
                ButtonMenuContext.active.background = TexturesLigthen[4];
                ButtonMenuContext.active.textColor = Color.white;
                ButtonMenuContext.alignment = TextAnchor.MiddleLeft;
                ButtonMenuContext.fontSize = 12;
                ButtonMenuContext.fontStyle = FontStyle.Bold;
                ButtonMenuContext.padding = new RectOffset(15, 2, 2, 2);
                ButtonMenuContext.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                ButtonAlert = new GUIStyle();
                ButtonAlert.normal.background = Textures[3];
                ButtonAlert.normal.textColor = Color.white;
                ButtonAlert.hover.background = TexturesDarken[3];
                ButtonAlert.hover.textColor = Color.white;
                ButtonAlert.active.background = TexturesLigthen[3];
                ButtonAlert.active.textColor = Color.white;
                ButtonAlert.alignment = TextAnchor.MiddleCenter;
                ButtonAlert.fontSize = 12;
                ButtonAlert.fontStyle = FontStyle.Bold;
                ButtonAlert.padding = new RectOffset(5, 5, 10, 10);
                ButtonAlert.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                ButtonAlternative = new GUIStyle();
                ButtonAlternative.normal.background = Textures[2];
                ButtonAlternative.normal.textColor = Color.white;
                ButtonAlternative.hover.background = TexturesDarken[2];
                ButtonAlternative.hover.textColor = Color.white;
                ButtonAlternative.active.background = TexturesLigthen[2];
                ButtonAlternative.active.textColor = Color.white;
                ButtonAlternative.alignment = TextAnchor.MiddleCenter;
                ButtonAlternative.fontSize = 12;
                ButtonAlternative.fontStyle = FontStyle.Bold;
                ButtonAlternative.padding = new RectOffset(5, 5, 10, 10);
                ButtonAlternative.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                ButtonGreen = new GUIStyle();
                ButtonGreen.normal.background = Textures[4];
                ButtonGreen.normal.textColor = Color.white;
                ButtonGreen.hover.background = TexturesDarken[4];
                ButtonGreen.hover.textColor = Color.white;
                ButtonGreen.active.background = TexturesLigthen[4];
                ButtonGreen.active.textColor = Color.white;
                ButtonGreen.alignment = TextAnchor.MiddleCenter;
                ButtonGreen.fontSize = 12;
                ButtonGreen.fontStyle = FontStyle.Bold;
                ButtonGreen.padding = new RectOffset(5, 5, 10, 10);
                ButtonGreen.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                Background0 = new GUIStyle();
                Background0.normal.background = Textures[0];
                Background0.normal.textColor = Color.white;
                Background0.alignment = TextAnchor.MiddleCenter;
                Background0.fontSize = 15;
                Background0.fontStyle = FontStyle.Bold;
                Background0.padding = new RectOffset(0, 0, 0, 0);
                Background0.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                Background5 = new GUIStyle();
                Background5.normal.background = Textures[5];
                Background5.normal.textColor = Color.white;
                Background5.alignment = TextAnchor.MiddleCenter;
                Background5.fontSize = 15;
                Background5.fontStyle = FontStyle.Bold;
                Background5.padding = new RectOffset(0, 0, 0, 0);
                Background5.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                Background8 = new GUIStyle();
                Background8.normal.background = Textures[8];
                Background8.normal.textColor = Color.white;
                Background8.alignment = TextAnchor.MiddleCenter;
                Background8.fontSize = 15;
                Background8.fontStyle = FontStyle.Bold;
                Background8.padding = new RectOffset(0, 0, 0, 0);
                Background8.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                Background1 = new GUIStyle();
                Background1.normal.background = Textures[1];
                Background1.normal.textColor = Color.white;
                Background1.alignment = TextAnchor.MiddleCenter;
                Background1.fontSize = 10;
                Background1.fontStyle = FontStyle.Bold;
                Background1.padding = new RectOffset();
                Background1.margin = new RectOffset();
            }

            {
                Background2 = new GUIStyle();
                Background2.normal.background = Textures[2];
                Background2.normal.textColor = Color.white;
                Background2.alignment = TextAnchor.MiddleCenter;
                Background2.fontSize = 15;
                Background2.fontStyle = FontStyle.Bold;
                Background2.padding = new RectOffset();
                Background2.margin = new RectOffset();
            }

            {
                Background3 = new GUIStyle();
                Background3.normal.background = Textures[3];
                Background3.normal.textColor = Color.white;
                Background3.alignment = TextAnchor.MiddleCenter;
                Background3.fontSize = 15;
                Background3.fontStyle = FontStyle.Bold;
                Background3.padding = new RectOffset();
                Background3.margin = new RectOffset();
            }

            {
                Background4 = new GUIStyle();
                Background4.normal.background = Textures[4];
                Background4.normal.textColor = Color.white;
                Background4.alignment = TextAnchor.MiddleCenter;
                Background4.fontSize = 15;
                Background4.fontStyle = FontStyle.Bold;
                Background4.padding = new RectOffset();
                Background4.margin = new RectOffset();
            }

            {
                Field = new GUIStyle();
                Field.normal.background = Textures[1];
                Field.normal.textColor = Color.white;
                Field.alignment = TextAnchor.MiddleLeft;
                Field.fontSize = 10;
                Field.fontStyle = FontStyle.Bold;
                Field.padding = new RectOffset(10, 10, 10, 10);
                Field.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                SeparatorField = new GUIStyle();
                SeparatorField.normal.background = Textures[5];
                SeparatorField.normal.textColor = Color.white;
                SeparatorField.alignment = TextAnchor.MiddleLeft;
                SeparatorField.fontSize = 10;
                SeparatorField.fontStyle = FontStyle.Bold;
                SeparatorField.padding = new RectOffset(10, 10, 10, 10);
                SeparatorField.margin = new RectOffset(0, 0, 0, 0);
            }

            {
                LibraryInfo = new GUIStyle();
                LibraryInfo.normal.background = Textures[1];
                LibraryInfo.normal.textColor = Color.white;
                LibraryInfo.alignment = TextAnchor.MiddleLeft;
                LibraryInfo.fontSize = 15;
                LibraryInfo.padding = new RectOffset(10, 10, 10, 10);
                LibraryInfo.margin = new RectOffset(0, 0, 0, 5);
            }

            {
                FieldValue = new GUIStyle();
                FieldValue.normal.background = Textures[0];
                FieldValue.normal.textColor = Color.white;

                FieldValue.alignment = TextAnchor.MiddleLeft;
                FieldValue.fontSize = 10;
                FieldValue.fontStyle = FontStyle.Bold;
                FieldValue.padding = new RectOffset(10, 10, 10, 10);
                FieldValue.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                LabelDate = new GUIStyle();
                LabelDate.normal.textColor = Color.white;
                LabelDate.alignment = TextAnchor.MiddleRight;
                LabelDate.fontSize = 10;
                LabelDate.fontStyle = FontStyle.Bold;
                LabelDate.padding = new RectOffset(10, 10, 10, 10);
                LabelDate.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                LabelPivot = new GUIStyle();
                LabelPivot.normal.textColor = Color.white;
                LabelPivot.alignment = TextAnchor.MiddleLeft;
                LabelPivot.fontSize = 10;
                LabelPivot.fontStyle = FontStyle.Bold;
                LabelPivot.padding = new RectOffset(1, 10, 10, 10);
                LabelPivot.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                FieldEnum = new GUIStyle();
                FieldEnum.normal.background = Textures[2];
                FieldEnum.normal.textColor = Color.white;

                FieldEnum.alignment = TextAnchor.MiddleLeft;
                FieldEnum.fontSize = 10;
                FieldEnum.fontStyle = FontStyle.Bold;
                FieldEnum.padding = new RectOffset(10, 10, 10, 10);
                FieldEnum.margin = new RectOffset(0, 0, 5, 0);
            }

            {
                ToggleBackgroundFalse = new GUIStyle();
                ToggleBackgroundFalse.normal.background = Textures[3];
            }

            {
                ToggleBackgroundTrue = new GUIStyle();
                ToggleBackgroundTrue.normal.background = Textures[4];
            }

            {
                ToggleBackgroundControl = new GUIStyle();
                ToggleBackgroundControl.normal.background = Textures[6];
            }

            {
                TimeLine = new GUIStyle();
                TimeLine.normal.background = Textures[0];
                TimeLine.normal.textColor = Color.white;

                TimeLine.alignment = TextAnchor.MiddleLeft;
                TimeLine.fontSize = 10;
                TimeLine.fontStyle = FontStyle.Bold;
                TimeLine.padding = new RectOffset(10, 10, 10, 10);
                TimeLine.margin = new RectOffset(0, 0, 0, 0);
            }
        }

        public Texture2D GetIcon(string name)
        {
            return Icon.Where(x =>x != null &&  x.name.Equals(name)).Take(1).SingleOrDefault();
        }

        private Texture2D LoadTexture(string b64, string name)
        {
            Texture2D texture = LoadTexture(name);
            if (texture != null)
                return texture;

            byte[] bytes = Convert.FromBase64String(b64);
            texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            texture.name = name;
            texture.Apply();
            return SaveTexture(texture);
        }

        private Texture2D LoadTexture(string b64, string name, Color color)
        {
            Texture2D texture = LoadTexture(name);
            if (texture != null)
                return texture;


            byte[] bytes = Convert.FromBase64String(b64);
            texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color c = texture.GetPixel(x, y);
                    c = new Color(color.r, color.g, color.b, c.a);
                    texture.SetPixel(x, y, c);
                }
            }
            texture.name = name;
            texture.Apply();            
            return SaveTexture(texture);
        }

        private Texture2D CreateTexture(Color color, string name)
        {
            Texture2D texture = LoadTexture(name);
            if (texture != null)
                return texture;

            if (texture == null)
            {
                texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, color);
                texture.name = name;
                texture.Apply();
                texture = SaveTexture(texture);
            }
            return texture;
        }

        private Texture2D LoadTexture(string name)
        {
            string path = "Assets/Plugins/SpriteAnimator/Editor/" + this.name + ".asset";
            UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            for(int i = 0; i < objects.Length; i++)
            {
                if(objects[i].name.Equals(name))
                {
                    return (Texture2D)objects[i];
                }
            }
            return null;
        }

        private Texture2D SaveTexture(Texture2D texture)
        {
            AssetDatabase.AddObjectToAsset(texture, this);
            AssetDatabase.SaveAssets();
            return LoadTexture(texture.name);
        }

        internal static Color32 CreateColor(int r, int g, int b, float alfa = 1)
        {
            return new Color(r / 255f, g / 255f, b / 255f, alfa);
        }        
    }

    public static class ColorExtension
    {
        internal static Color Darken(this Color color, float value)
        {
            color = new Color(color.r, color.g, color.b, color.a);
            color.r -= value / 255f;
            color.g -= value / 255f;
            color.b -= value / 255f;

            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);
            return color;
        }

        internal static Color Ligthen(this Color color, float value)
        {
            color = new Color(color.r, color.g, color.b, color.a);
            color.r += value / 255f;
            color.g += value / 255f;
            color.b += value / 255f;

            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);
            return color;
        }
    }
}
